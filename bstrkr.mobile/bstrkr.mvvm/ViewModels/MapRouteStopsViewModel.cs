using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using bstrkr.core;
using bstrkr.core.config;
using bstrkr.core.map;
using bstrkr.core.spatial;
using bstrkr.providers;

using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;

namespace bstrkr.mvvm.viewmodels
{
    public class MapRouteStopsViewModel : BusTrackerViewModelBase
    {
        private const double MaxDistanceFromBusStop = 500.0;

        private readonly BusTrackerConfig _config;
        private readonly ObservableCollection<RouteStopMapViewModel> _stops = new ObservableCollection<RouteStopMapViewModel>();

        private ILiveDataProvider _liveDataProvider;

        private float _zoom;
        private GeoPoint _mapCenter = GeoPoint.Empty;
        private RouteStopMapViewModel _selectedRouteStop;
        private MapMarkerSizes _markerSize = MapMarkerSizes.Medium;

        public MapRouteStopsViewModel(IConfigManager configManager)
        {
            this.Stops = new ReadOnlyObservableCollection<RouteStopMapViewModel>(_stops);
            _config = configManager.GetConfig();

            this.LoadRouteStopsCommand = new MvxCommand(this.LoadRouteStops, () => _liveDataProvider != null);
            this.SelectRouteStopCommand = new MvxCommand<string>(this.SelectRouteStop);
        }

        public event EventHandler<RouteStopSelectedEventArgs> RouteStopSelected;

        public MvxCommand LoadRouteStopsCommand { get; }

        public MvxCommand<string> SelectRouteStopCommand { get; }

        public ReadOnlyObservableCollection<RouteStopMapViewModel> Stops { get; private set; }

        public RouteStopMapViewModel SelectedRouteStop
        {
            get { return _selectedRouteStop; }
            private set
            {
                if (_selectedRouteStop != value)
                {
                    _selectedRouteStop = value;
                    this.RaisePropertyChanged(() => this.SelectedRouteStop);
                }
            }
        }

        public MapMarkerSizes MarkerSize
        {
            get { return _markerSize; }
            set
            {
                if (_markerSize != value)
                {
                    _markerSize = value;
                    this.RaisePropertyChanged(() => this.MarkerSize);
                }
            }
        }

        public float Zoom
        {
            get { return _zoom; }
            set
            {
                if (_zoom != value)
                {
                    _zoom = value;
                    this.RaisePropertyChanged(() => this.Zoom);
                    this.OnZoomChanged(value);
                }
            }
        }

        public GeoPoint MapCenter
        {
            get { return _mapCenter; }
            set
            {
                if (!_mapCenter.Equals(value))
                {
                    _mapCenter = value;
                    this.RaisePropertyChanged(() => this.MapCenter);
                }
            }
        }

        public void Initialize(ILiveDataProvider provider)
        {
            this.Clear();
            _liveDataProvider = provider;
        }

        private void Clear()
        {
            _stops.Clear();
            _selectedRouteStop = null;
        }

        private void LoadRouteStops()
        {
            _liveDataProvider.GetRouteStopsAsync()
                             .ContinueWith(this.ShowRouteStops)
                             .ConfigureAwait(false);
        }

        private void OnZoomChanged(float zoom)
        {
            var isRouteStopVisible = this.IsRouteStopVisible(zoom);
            foreach (var routeStopVM in _stops)
            {
                routeStopVM.IsVisible = isRouteStopVisible;
            }
        }

        private void ShowRouteStops(Task<IEnumerable<RouteStop>> task)
        {
            if (task.Status == TaskStatus.RanToCompletion && task.Result != null)
            {
                this.Dispatcher.RequestMainThreadAction(() =>
                {
                    foreach (var stop in task.Result)
                    {
                        var vm = this.CreateRouteStopVM(stop);
                        _stops.Add(vm);
                    }

                    this.SelectClosestRouteStop(this.MapCenter);
                });
            }
            else
            {
                Task.Delay(TimeSpan.FromSeconds(10))
                    .ContinueWith(delayTask => Task.Factory.StartNew(this.LoadRouteStops))
                    .ConfigureAwait(false);
            }
        }

        private void SelectClosestRouteStop(GeoPoint location)
        {
            var closestStop = _stops.Select(x => x.Model)
                                    .Select(x => new Tuple<double, RouteStop>(location.DistanceTo(x.Location.Position), x))
                                    .OrderBy(x => x.Item1)
                                    .First();

            if (closestStop.Item1 <= MaxDistanceFromBusStop)
            {
                var closestRouteStop = closestStop.Item2;
                this.Dispatcher.RequestMainThreadAction(() => this.SelectRouteStop(closestRouteStop.Id));
            }
        }

        private RouteStopMapViewModel CreateRouteStopVM(RouteStop routeStop)
        {
            var stopVM = Mvx.IocConstruct<RouteStopMapViewModel>();
            stopVM.Model = routeStop;
            stopVM.MarkerSize = MapMarkerSizes.Medium;

            if (_selectedRouteStop != null)
            {
                stopVM.SelectionState = MapMarkerSelectionStates.SelectionNotSelected;
            }

            return stopVM;
        }

        private void SelectRouteStop(string routeStopId)
        {
            if (string.IsNullOrEmpty(routeStopId))
            {
                this.SelectedRouteStop = null;
                this.SetRouteStopMarkersSelectionState(MapMarkerSelectionStates.SelectionNotSelected, null);

                return;
            }

            var routeStopVM = _stops.FirstOrDefault(x => x.Model.Id.Equals(routeStopId));
            if (routeStopVM == null)
            {
                return;
            }

            this.SelectedRouteStop = routeStopVM;
            this.SelectedRouteStop.SelectionState = MapMarkerSelectionStates.SelectionSelected;

            this.SetRouteStopMarkersSelectionState(MapMarkerSelectionStates.SelectionNotSelected, new[] { _selectedRouteStop.Model.Id });

            var requestedBy = new MvxRequestedBy(MvxRequestedByType.UserAction, "map_tap");
            this.ShowViewModel<RouteStopViewModel>(
                                                new
                                                {
                                                    id = _selectedRouteStop.Model.Id,
                                                    name = _selectedRouteStop.Model.Name,
                                                    description = _selectedRouteStop.Model.Description
                                                },
                                                null,
                                                requestedBy);

            this.RaiseRouteStopSelectedEvent(this.SelectedRouteStop);
        }

        private bool IsRouteStopVisible(float zoom)
        {
            return zoom > _config.ShowRouteStopsZoomThreshold;
        }

        private void SetRouteStopMarkersSelectionState(
                            MapMarkerSelectionStates selectionState,
                            IEnumerable<string> excludeStops = null)
        {
            foreach (var stop in _stops)
            {
                if (excludeStops != null)
                {
                    if (!excludeStops.Any(v => v.Equals(stop.Model.Id)))
                    {
                        stop.SelectionState = selectionState;
                    }
                }
                else
                {
                    stop.SelectionState = selectionState;
                }
            }
        }

        private void RaiseRouteStopSelectedEvent(RouteStopMapViewModel routeStop)
        {
            this.RouteStopSelected?.Invoke(this, new RouteStopSelectedEventArgs(routeStop));
        }
    }
}