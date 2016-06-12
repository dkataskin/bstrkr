using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;

using bstrkr.core;
using bstrkr.core.config;
using bstrkr.core.map;
using bstrkr.core.spatial;
using bstrkr.mvvm.converters;
using bstrkr.mvvm.messages;
using bstrkr.providers;

using Cirrious.CrossCore;
using Cirrious.CrossCore.Platform;
using Cirrious.MvvmCross.Plugins.Messenger;
using Cirrious.MvvmCross.ViewModels;

using Xamarin;

namespace bstrkr.mvvm.viewmodels
{
    public class MapVehiclesViewModel : BusTrackerViewModelBase
    {
        private const string UpdateVehicleLocationsTaskId = "UpdateVehicleLocations";

        private readonly IMvxMessenger _messenger;
        private readonly BusTrackerConfig _config;
        private readonly ISubject<VehiclesViewPortUpdate> _viewPortUpdateSubject;
        private readonly MvxSubscriptionToken _updateVehicleLocationsSubscriptionToken;
        private readonly ZoomToVisibleRegionExpandRatioConverter _zoomToVisibleRegionExpandRatioConverter = new ZoomToVisibleRegionExpandRatioConverter();
        private readonly ConcurrentDictionary<string, VehicleViewModel> _vehicles = new ConcurrentDictionary<string, VehicleViewModel>();
        private readonly Dictionary<string, VehicleViewModel> _visibleVehicles = new Dictionary<string, VehicleViewModel>();

        private GeoRect _viewport;
        private ILiveDataProvider _liveDataProvider;
        private VehicleViewModel _selectedVehicle;
        private IDisposable _subscription;
        private float _zoom;
        private MapMarkerSizes _markerSize = MapMarkerSizes.Medium;

        public MapVehiclesViewModel(IMvxMessenger messenger, IConfigManager configManager)
        {
            _messenger = messenger;
            _config = configManager.GetConfig();
            _viewPortUpdateSubject = new Subject<VehiclesViewPortUpdate>();
            this.ViewportUpdate = new Subject<VisibleVehiclesDelta>();

            _updateVehicleLocationsSubscriptionToken = _messenger.Subscribe<VehicleLocationsUpdateRequestMessage>(
                                                                            message => this.ForceVehicleLocationsUpdate());

            _subscription = _viewPortUpdateSubject.Throttle(TimeSpan.FromMilliseconds(200))
                                                  .Subscribe(this.UpdateVehiclesInTheViewPort);

            this.ForceVehicleLocationsUpdateCommand = new MvxCommand(this.ForceVehicleLocationsUpdate);
            this.SelectVehicleCommand = new MvxCommand<string>(this.SelectVehicle);
        }

        public MvxCommand ForceVehicleLocationsUpdateCommand { get; private set; }

        public MvxCommand<string> SelectVehicleCommand { get; private set;  }

        public ISubject<VisibleVehiclesDelta> ViewportUpdate { get; }

        public GeoRect Viewport
        {
            get { return _viewport; }
            set
            {
                _viewport = value;
                this.RaisePropertyChanged(() => this.Viewport);
                this.OnViewportUpdated(value);
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

        public void Initialize(ILiveDataProvider liveDataProvider)
        {
            _vehicles.Clear();
            _visibleVehicles.Clear();

            if (_liveDataProvider != null)
            {
                _liveDataProvider.VehicleLocationsUpdateStarted -= this.OnVehicleLocationsUpdateStarted;
                _liveDataProvider.VehicleLocationsUpdateFailed -= this.OnVehicleLocationsUpdateFailed;
                _liveDataProvider.VehicleLocationsUpdated -= this.OnVehicleLocationsUpdated;
            }

            _liveDataProvider = liveDataProvider;
            _liveDataProvider.VehicleLocationsUpdateStarted += this.OnVehicleLocationsUpdateStarted;
            _liveDataProvider.VehicleLocationsUpdateFailed += this.OnVehicleLocationsUpdateFailed;
            _liveDataProvider.VehicleLocationsUpdated += this.OnVehicleLocationsUpdated;
        }

        private void OnViewportUpdated(GeoRect viewport)
        {
            _viewPortUpdateSubject.OnNext(this.GenerateUpdateFrom(viewport, this.Zoom));
        }

        private void OnVehicleLocationsUpdated(object sender, VehicleLocationsUpdatedEventArgs args)
        {
            try
            {
                MvxTrace.Trace("vehicle locations received, count={0}", args.VehicleLocations.Count);
                _messenger.Publish(new BackgroundTaskStateChangedMessage(this, UpdateVehicleLocationsTaskId, BackgroundTaskState.Finished));

                _viewPortUpdateSubject.OnNext(this.GenerateUpdateFrom(args.VehicleLocations, this.Viewport, this.Zoom));
            }
            catch (Exception e)
            {
                Insights.Report(e);
            }
        }

        private VehicleViewModel CreateVehicleVM(VehicleLocationUpdate locationUpdate)
        {
            try
            {
                var vehicleVM = Mvx.IocConstruct<VehicleViewModel>();
                vehicleVM.Model = locationUpdate.Vehicle;
                vehicleVM.MarkerSize = _markerSize;
                vehicleVM.AnimateMovement = this.IsAnimationEnabled(this.Zoom);
                vehicleVM.IsTitleVisible = this.IsVehicleTitleMarkerVisible(this.Zoom);

                if (_selectedVehicle != null)
                {
                    vehicleVM.SelectionState = MapMarkerSelectionStates.SelectionNotSelected;
                }

                return vehicleVM;
            }
            catch (Exception e)
            {
                Insights.Report(e);
                return null;
            }
        }

        private void UpdateVehicleVM(VehicleViewModel vehicleVM, VehicleLocationUpdate locationUpdate)
        {
            vehicleVM.AnimateMovement = this.IsAnimationEnabled(this.Zoom);
            vehicleVM.Update(locationUpdate);
        }

        private void OnVehicleLocationsUpdateStarted(object sender, EventArgs eventArgs)
        {
            _messenger.Publish(new BackgroundTaskStateChangedMessage(this, UpdateVehicleLocationsTaskId, BackgroundTaskState.Running));
        }

        private void OnVehicleLocationsUpdateFailed(object sender, EventArgs eventArgs)
        {
            _messenger.Publish(new BackgroundTaskStateChangedMessage(this, UpdateVehicleLocationsTaskId, BackgroundTaskState.Failed));
        }

        private void ForceVehicleLocationsUpdate()
        {
            var provider = _liveDataProvider;
            provider?.UpdateVehicleLocationsAsync();
        }

        private void SelectVehicle(string vehicleId)
        {
            if (string.IsNullOrEmpty(vehicleId))
            {
                _selectedVehicle = null;
                this.SetVehicleMarkersSelectionState(MapMarkerSelectionStates.SelectionNotSelected, null);
            }

            VehicleViewModel vehicleVM;
            _vehicles.TryGetValue(vehicleId, out vehicleVM);
            if (vehicleVM == null)
            {
                return;
            }

            _selectedVehicle = vehicleVM;
            _selectedVehicle.SelectionState = MapMarkerSelectionStates.SelectionSelected;

            this.SetVehicleMarkersSelectionState(MapMarkerSelectionStates.SelectionNotSelected, new[] { _selectedVehicle.Model.Id });

            var requestedBy = new MvxRequestedBy(MvxRequestedByType.UserAction, "map_tap");
            var navParams = new
            {
                id = _selectedVehicle.VehicleId,
                carPlate = _selectedVehicle.CarPlate,
                vehicleType = _selectedVehicle.VehicleType,
                routeId = _selectedVehicle.Model.RouteInfo.RouteId,
                routeNumber = _selectedVehicle.Model.RouteInfo.RouteNumber,
                routeDisplayName = _selectedVehicle.Model.RouteInfo.DisplayName,
                runUpdates = true
            };

            this.ShowViewModel<VehicleForecastViewModel>(navParams, null, requestedBy);

            //TODO: center map view on vehicle
            //this.CenterMap(_selectedVehicle.Location.Position);
        }

        private void OnZoomChanged(float zoom)
        {
            foreach (var vm in _vehicles.Values)
            {
                vm.MarkerSize = _markerSize;
                vm.AnimateMovement = this.IsAnimationEnabled(zoom);
                vm.IsTitleVisible = this.IsVehicleTitleMarkerVisible(zoom);
            }
        }

        private void UpdateVehiclesInTheViewPort(VehiclesViewPortUpdate update)
        {
            MvxTrace.Trace("Updating vehicles in the viewport... ");

            try
            {
                this.UpdateVehicles(update.VehicleUpdates);
                var mapUpdate = this.UpdateVisibleVehicles(update);

                this.ViewportUpdate.OnNext(mapUpdate);
            }
            catch (Exception ex)
            {
                Insights.Report(ex, Insights.Severity.Error);
            }
        }

        private VisibleVehiclesDelta UpdateVisibleVehicles(VehiclesViewPortUpdate update)
        {
            var visibleVehiclesNew = new Dictionary<string, VehicleViewModel>();
            foreach (var keyValuePair in _vehicles)
            {
                if (update.VisibleRegion.ContainsPoint(keyValuePair.Value.LocationAnimated))
                {
                    visibleVehiclesNew.Add(keyValuePair.Key, keyValuePair.Value);
                }
            }

            MvxTrace.Trace("{0} visible vehicles, {1} current vehicles are visible", visibleVehiclesNew.Count, _visibleVehicles.Count);

            var vehiclesToRemove = new List<VehicleViewModel>();
            foreach (var keyValuePair in _visibleVehicles)
            {
                if (visibleVehiclesNew.ContainsKey(keyValuePair.Key))
                {
                    visibleVehiclesNew.Remove(keyValuePair.Key);
                }
                else
                {
                    vehiclesToRemove.Add(keyValuePair.Value);
                }
            }

            var animateMovement = this.IsAnimationEnabled(update.Zoom);
            MvxTrace.Trace("{0} vehicles should be removed, {1} added", vehiclesToRemove.Count, visibleVehiclesNew.Count);
            foreach (var vm in vehiclesToRemove)
            {
                _visibleVehicles.Remove(vm.VehicleId);
                this.ViewDispatcher.RequestMainThreadAction(() =>
                {
                    vm.IsInView = false;
                    vm.AnimateMovement = false;
                });
            }

            foreach (var vm in visibleVehiclesNew.Values)
            {
                _visibleVehicles[vm.VehicleId] = vm;
                this.ViewDispatcher.RequestMainThreadAction(() =>
                {
                    vm.IsInView = true;
                    vm.AnimateMovement = animateMovement;
                });
            }

            return new VisibleVehiclesDelta(visibleVehiclesNew.Values.ToList(), vehiclesToRemove);
        }

        private void UpdateVehicles(IEnumerable<VehicleLocationUpdate> updates)
        {
            foreach (var vehicleUpdate in updates)
            {
                VehicleViewModel vehicleVM = null;
                _vehicles.TryGetValue(vehicleUpdate.Vehicle.Id, out vehicleVM);
                if (vehicleVM != null)
                {
                    this.UpdateVehicleVM(vehicleVM, vehicleUpdate);
                }
                else
                {
                    var vm = this.CreateVehicleVM(vehicleUpdate);
                    if (vm != null)
                    {
                        _vehicles.TryAdd(vehicleUpdate.Vehicle.Id, vm);
                    }
                }
            }
        }

        private VehiclesViewPortUpdate GenerateUpdateFrom(GeoRect visibleRegionUpdate, float zoom)
        {
            return new VehiclesViewPortUpdate
            {
                VehicleUpdates = new List<VehicleLocationUpdate>(),
                VisibleRegion = this.ExpandVisibleRegion(visibleRegionUpdate, zoom),
                Zoom = zoom
            };
        }

        private VehiclesViewPortUpdate GenerateUpdateFrom(
                                                IReadOnlyCollection<VehicleLocationUpdate> vehicleLocationUpdates,
                                                GeoRect visibleRegionUpdate,
                                                float zoom)
        {
            return new VehiclesViewPortUpdate
            {
                VehicleUpdates = vehicleLocationUpdates,
                VisibleRegion = this.ExpandVisibleRegion(visibleRegionUpdate, zoom),
                Zoom = zoom
            };
        }

        private GeoRect ExpandVisibleRegion(GeoRect visibleRegion, float zoom)
        {
            var ratio = (float)_zoomToVisibleRegionExpandRatioConverter.Convert(
                                                                    zoom,
                                                                    typeof(float),
                                                                    null,
                                                                    CultureInfo.InvariantCulture);

            if (ratio == 1.0f)
            {
                return visibleRegion;
            }

            var dLat = ratio * Math.Abs(visibleRegion.NorthEast.Latitude - visibleRegion.SouthWest.Latitude) / 2.0f;
            var dLon = ratio * Math.Abs(visibleRegion.NorthEast.Longitude - visibleRegion.SouthWest.Longitude) / 2.0f;

            var northEast = new GeoPoint(visibleRegion.NorthEast.Latitude + dLat, visibleRegion.NorthEast.Longitude + dLon);
            var southWest = new GeoPoint(visibleRegion.SouthWest.Latitude - dLat, visibleRegion.SouthWest.Longitude - dLon);

            return new GeoRect(northEast, southWest);
        }

        private bool IsAnimationEnabled(float zoom)
        {
            return Settings.AnimateMarkers && zoom > _config.AnimateMarkersMovementZoomThreshold;
        }

        private bool IsVehicleTitleMarkerVisible(float zoom)
        {
            return zoom > _config.ShowVehicleTitlesZoomThreshold;
        }

        private void SetVehicleMarkersSelectionState(
                            MapMarkerSelectionStates selectionState,
                            IEnumerable<string> excludeVehicles = null)
        {
            foreach (var vehicleId in _vehicles.Keys)
            {
                VehicleViewModel vehicleVM = null;
                _vehicles.TryGetValue(vehicleId, out vehicleVM);
                if (vehicleVM == null || (excludeVehicles != null && excludeVehicles.Contains(vehicleId)))
                {
                    continue;
                }

                vehicleVM.SelectionState = selectionState;
            }
        }

        private class VehiclesViewPortUpdate
        {
            public IEnumerable<VehicleLocationUpdate> VehicleUpdates { get; set; }

            public GeoRect VisibleRegion { get; set; }

            public float Zoom { get; set; }
        }
    }
}