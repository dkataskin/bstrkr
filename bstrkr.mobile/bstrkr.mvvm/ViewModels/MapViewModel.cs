using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using bstrkr.core;
using bstrkr.core.config;
using bstrkr.core.map;
using bstrkr.core.services.location;
using bstrkr.core.spatial;
using bstrkr.mvvm.converters;
using bstrkr.mvvm.messages;
using bstrkr.providers;

using Cirrious.CrossCore;
using Cirrious.CrossCore.Platform;
using Cirrious.MvvmCross.Plugins.Messenger;
using Cirrious.MvvmCross.ViewModels;

namespace bstrkr.mvvm.viewmodels
{
    public class MapViewModel : BusTrackerViewModelBase
    {
        private const double MaxDistanceFromBusStop = 500.0;

        private readonly ZoomToMarkerSizeConverter _zoomToMarkerSizeConverter = new ZoomToMarkerSizeConverter();
        private readonly IBusTrackerLocationService _locationService;
        private readonly ILiveDataProviderFactory _providerFactory;
        private readonly IMvxMessenger _messenger;
        private readonly IConfigManager _configManager;
        private readonly MvxSubscriptionToken _routeStopInfoSubscriptionToken;
        private readonly MvxSubscriptionToken _vehicleInfoSubscriptionToken;
        private readonly BusTrackerConfig _config;

        private readonly ObservableCollection<RouteStopMapViewModel> _stops = new ObservableCollection<RouteStopMapViewModel>();

        private MapMarkerSizes _markerSize = MapMarkerSizes.Medium;
        private ILiveDataProvider _liveDataProvider;
        private GeoPoint _mapCenter = GeoPoint.Empty;
        private GeoRect _visibleRegion;
        private bool _detectedArea = false;
        private float _zoom;
        private RouteStop _routeStop;
        private RouteStopMapViewModel _selectedRouteStop;

        public MapViewModel(
                    IBusTrackerLocationService locationService,
                    ILiveDataProviderFactory providerFactory,
                    IConfigManager configManager,
                    IMvxMessenger messenger)
        {
            _providerFactory = providerFactory;
            _configManager = configManager;
            _messenger = messenger;
            _locationService = locationService;
            _locationService.AreaChanged += (s, a) =>
            {
                this.DetectedArea = a.Detected;
                this.ChangeArea(a.Area, a.LastLocation);
            };

            _config = _configManager.GetConfig();

            this.Stops = new ReadOnlyObservableCollection<RouteStopMapViewModel>(_stops);
            this.SelectRouteStopCommand = new MvxCommand<string>(this.SelectRouteStop);
            this.ClearSelectionCommand = new MvxCommand(this.ClearSelection);

            _routeStopInfoSubscriptionToken = _messenger.Subscribe<ShowRouteStopForecastOnMapMessage>(
                                                    message => this.SelectRouteStopCommand.Execute(message.RouteStopId));

            _vehicleInfoSubscriptionToken = _messenger.Subscribe<ShowVehicleForecastOnMapMessage>(
                                                message => this.SelectVehicleCommand.Execute(message.VehicleId));

            this.UpdateMapCenterCommand = new MvxCommand<Tuple<GeoPoint, bool>>(tuple =>
            {
                if (tuple.Item2)
                {
                    this.MapCenter = tuple.Item1;
                }
                else
                {
                    _mapCenter = tuple.Item1;
                }
            });
            this.SelectVehicleCommand = new MvxCommand<string>(this.SelectVehicle);
        }

        public MvxCommand<string> SelectRouteStopCommand { get; }

        public MvxCommand<string> SelectVehicleCommand { get; } 

        public MvxCommand ClearSelectionCommand { get; }

        public MvxCommand<Tuple<GeoPoint, bool>> UpdateMapCenterCommand { get; private set; }

        public ReadOnlyObservableCollection<RouteStopMapViewModel> Stops { get; private set; }

        public GeoPoint MapCenter
        {
            get { return _mapCenter; }
            private set
            {
                if (!_mapCenter.Equals(value))
                {
                    _mapCenter = value;
                    this.RaisePropertyChanged(() => this.MapCenter);
                }
            }
        }

        public GeoRect VisibleRegion
        {
            get { return _visibleRegion; }
            set
            {
                this.RaiseAndSetIfChanged(ref _visibleRegion, value, () => this.VisibleRegion);
                if (!_visibleRegion.Equals(value))
                {
                    //_viewPortUpdateSubject.OnNext(this.GenerateUpdateFrom(this.VisibleRegion, this.Zoom));
                }
            }
        }

        public bool DetectedArea
        {
            get { return _detectedArea; }
            private set
            {
                if (_detectedArea != value)
                {
                    _detectedArea = value;
                    this.RaisePropertyChanged(() => this.DetectedArea);
                }
            }
        }

        public RouteStop RouteStop
        {
            get { return _routeStop; }
            private set
            {
                if (_routeStop != value)
                {
                    _routeStop = value;
                    this.RaisePropertyChanged(() => this.RouteStop);
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

        public override void Start()
        {
            base.Start();

            if (_locationService.CurrentArea != null)
            {
                this.DetectedArea = _locationService.DetectedArea;
                this.ChangeArea(_locationService.CurrentArea, _locationService.GetLastLocation());
            }

            this.IsBusy = true;
        }

        public void Reload()
        {
            _liveDataProvider?.Start();
        }

        public void Stop()
        {
            _liveDataProvider?.Stop();
        }

        private void ChangeArea(Area area, GeoPoint centerPosition)
        {
            MvxTrace.Trace("Area changed to {0}", area != null ? area.Id : string.Empty);

            this.MapCenter = centerPosition;
            _liveDataProvider?.Stop();

            _stops.Clear();

            this.InitializeStartLiveDataProvider(_providerFactory);

            this.IsBusy = false;
        }

        private void InitializeStartLiveDataProvider(ILiveDataProviderFactory factory)
        {
            _liveDataProvider = factory.GetCurrentProvider();
            if (_liveDataProvider != null)
            {
                //  TODO: initialize vehicles vm
                _liveDataProvider.GetRouteStopsAsync()
                                 .ContinueWith(this.ShowRouteStops)
                                 .ConfigureAwait(false);

                _liveDataProvider.Start();

                MvxTrace.Trace(() => "provider started");
            }
        }

        private void ShowRouteStops(Task<IEnumerable<RouteStop>> task)
        {
            if (task.Status == TaskStatus.RanToCompletion && task.Result != null)
            {
                this.Dispatcher.RequestMainThreadAction(() =>
                {
                    lock (_stops)
                    {
                        foreach (var stop in task.Result)
                        {
                            var vm = this.CreateRouteStopVM(stop);
                            _stops.Add(vm);
                        }

                        this.SelectClosestRouteStop(this.MapCenter);
                    };
                });
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
                this.Dispatcher.RequestMainThreadAction(() => this.RouteStop = closestRouteStop);
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
                this.ClearSelection();
                return;
            }

            RouteStopMapViewModel routeStopVM;
            lock (_stops)
            {
                routeStopVM = _stops.FirstOrDefault(x => x.Model.Id.Equals(routeStopId));
            }

            if (routeStopVM == null)
            {
                return;
            }

            this.ClearSelection();

            _selectedRouteStop = routeStopVM;
            _selectedRouteStop.SelectionState = MapMarkerSelectionStates.SelectionSelected;

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

            this.CenterMap(_selectedRouteStop.Location.Position);
        }

        private void SelectVehicle(string vehicleId)
        {
            if (string.IsNullOrEmpty(vehicleId))
            {
                this.ClearSelectionCommand.Execute();
                return;
            }

            this.ClearSelectionCommand.Execute();

            // TODO: execute vehicles view models select vehicle command
        }

        private void OnZoomChanged(float zoom)
        {
            _markerSize = (MapMarkerSizes)_zoomToMarkerSizeConverter.Convert(
                                                                          zoom,
                                                                          typeof(MapMarkerSizes),
                                                                          null,
                                                                          CultureInfo.InvariantCulture);
            lock (_stops)
            {
                foreach (var routeStop in _stops)
                {
                    routeStop.IsVisible = this.IsRouteStopVisible(zoom);
                }
            }
        }

        private bool IsRouteStopVisible(float zoom)
        {
            return zoom > _config.ShowRouteStopsZoomThreshold;
        }

        private void SetRouteStopMarkersSelectionState(
                                    MapMarkerSelectionStates selectionState,
                                    IEnumerable<string> excludeStops = null)
        {
            lock (_stops)
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
        }

        private void SetMarkersSelectionState(
                        MapMarkerSelectionStates selectionState,
                        IEnumerable<string> excludeVehicles = null,
                        IEnumerable<string> excludeStops = null)
        {
            this.SetRouteStopMarkersSelectionState(selectionState, excludeStops);
            //this.SetVehicleMarkersSelectionState(selectionState, excludeVehicles);
        }

        private void ClearSelection()
        {
            this.SetMarkersSelectionState(MapMarkerSelectionStates.NoSelection);
        }

        private void CenterMap(GeoPoint location)
        {
            this.MapCenter = location;
        }
    }
}