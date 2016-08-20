using System;
using System.Globalization;

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

            this.MapRouteStopsViewModel = Mvx.IocConstruct<MapRouteStopsViewModel>();
            this.MapRouteStopsViewModel.RouteStopSelected += (s, a) => this.CenterMap(a.RouteStop.Location.Position);

            this.MapVehiclesViewModel = Mvx.IocConstruct<MapVehiclesViewModel>();

            this.SelectRouteStopCommand = new MvxCommand<string>(this.SelectRouteStop);
            this.SelectVehicleCommand = new MvxCommand<string>(this.SelectVehicle);
            this.ClearSelectionCommand = new MvxCommand(this.ClearSelection);
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

            _routeStopInfoSubscriptionToken = _messenger.Subscribe<ShowRouteStopForecastOnMapMessage>(
                                                    message => this.SelectRouteStopCommand.Execute(message.RouteStopId));

            _vehicleInfoSubscriptionToken = _messenger.Subscribe<ShowVehicleForecastOnMapMessage>(
                                                message => this.SelectVehicleCommand.Execute(message.VehicleId));
        }

        public MvxCommand<string> SelectRouteStopCommand { get; }

        public MvxCommand<string> SelectVehicleCommand { get; } 

        public MvxCommand ClearSelectionCommand { get; }

        public MvxCommand<Tuple<GeoPoint, bool>> UpdateMapCenterCommand { get; private set; }

        public MapRouteStopsViewModel MapRouteStopsViewModel { get; }

        public MapVehiclesViewModel MapVehiclesViewModel { get; }

        public GeoPoint MapCenter
        {
            get { return _mapCenter; }
            private set
            {
                if (!_mapCenter.Equals(value))
                {
                    _mapCenter = value;
                    this.MapRouteStopsViewModel.MapCenter = value;
                    this.RaisePropertyChanged(() => this.MapCenter);
                }
            }
        }

        public GeoRect VisibleRegion
        {
            get { return _visibleRegion; }
            set
            {
                if (!_visibleRegion.Equals(value))
                {
                    _visibleRegion = value;
                    this.RaisePropertyChanged(() => this.VisibleRegion);
                    this.MapVehiclesViewModel.Viewport = value;
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

                    this.MapRouteStopsViewModel.Zoom = value;
                    this.MapVehiclesViewModel.Zoom = value;
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

            this.InitializeStartLiveDataProvider(_providerFactory);

            this.IsBusy = false;
        }

        private void InitializeStartLiveDataProvider(ILiveDataProviderFactory factory)
        {
            _liveDataProvider = factory.GetCurrentProvider();
            if (_liveDataProvider != null)
            {
                this.MapRouteStopsViewModel.Initialize(_liveDataProvider);
                this.MapVehiclesViewModel.Initialize(_liveDataProvider);

                _liveDataProvider.Start();

                this.MapRouteStopsViewModel.LoadRouteStopsCommand.Execute();
                this.MapVehiclesViewModel.ForceVehicleLocationsUpdateCommand.Execute();

                MvxTrace.Trace(() => "provider started");
            }
        }

        private void SelectRouteStop(string routeStopId)
        {
            this.MapVehiclesViewModel.SelectVehicleCommand.Execute(string.Empty);
            this.MapRouteStopsViewModel.SelectRouteStopCommand.Execute(routeStopId);
        }

        private void SelectVehicle(string vehicleId)
        {
            this.MapRouteStopsViewModel.SelectRouteStopCommand.Execute(string.Empty);
            this.MapVehiclesViewModel.SelectVehicleCommand.Execute(vehicleId);
        }

        private void OnZoomChanged(float zoom)
        {
            _markerSize = (MapMarkerSizes)_zoomToMarkerSizeConverter.Convert(
                                                                          zoom,
                                                                          typeof(MapMarkerSizes),
                                                                          null,
                                                                          CultureInfo.InvariantCulture);
            this.MapRouteStopsViewModel.MarkerSize = _markerSize;
            this.MapVehiclesViewModel.MarkerSize = _markerSize;
        }

        private void ClearSelection()
        {
            this.MapVehiclesViewModel.SelectVehicleCommand.Execute(string.Empty);
            this.MapRouteStopsViewModel.SelectRouteStopCommand.Execute(string.Empty);
        }

        private void CenterMap(GeoPoint location)
        {
            this.MapCenter = location;
        }
    }
}