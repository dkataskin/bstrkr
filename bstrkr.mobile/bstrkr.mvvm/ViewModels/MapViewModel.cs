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
        private readonly ZoomToMarkerSizeConverter _zoomToMarkerSizeConverter = new ZoomToMarkerSizeConverter();
        private readonly IBusTrackerLocationService _locationService;
        private readonly ILiveDataProviderFactory _providerFactory;
        private readonly IMvxMessenger _messenger;
        private readonly IConfigManager _configManager;
        private readonly MvxSubscriptionToken _routeStopInfoSubscriptionToken;
        private readonly MvxSubscriptionToken _vehicleInfoSubscriptionToken;
        private readonly BusTrackerConfig _config;

        private MapMarkerSizes _markerSize = MapMarkerSizes.Medium;
        private ILiveDataProvider _liveDataProvider;
        private GeoPoint _mapCenter = GeoPoint.Empty;
        private GeoRect _visibleRegion;
        private bool _detectedArea = false;
        private float _zoom;

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

           //TODO: clear stops and vehicles

            this.InitializeStartLiveDataProvider(_providerFactory);

            this.IsBusy = false;
        }

        private void InitializeStartLiveDataProvider(ILiveDataProviderFactory factory)
        {
            _liveDataProvider = factory.GetCurrentProvider();
            if (_liveDataProvider != null)
            {
                //  TODO: initialize vehicles vm
                //  TODO: initialize stops vm
                _liveDataProvider.GetRouteStopsAsync()
                                 .ContinueWith(this.ShowRouteStops)
                                 .ConfigureAwait(false);

                _liveDataProvider.Start();

                MvxTrace.Trace(() => "provider started");
            }
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
            //TODO: cascade updates to stops and vehicles
        }

        private void SetMarkersSelectionState(
                        MapMarkerSelectionStates selectionState,
                        IEnumerable<string> excludeVehicles = null,
                        IEnumerable<string> excludeStops = null)
        {
            //this.SetRouteStopMarkersSelectionState(selectionState, excludeStops);
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