using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

using bstrkr.core;
using bstrkr.core.collections;
using bstrkr.core.config;
using bstrkr.core.map;
using bstrkr.core.services.location;
using bstrkr.core.spatial;
using bstrkr.mvvm.converters;
using bstrkr.mvvm.messages;
using bstrkr.mvvm.navigation;
using bstrkr.providers;

using Chance.MvvmCross.Plugins.UserInteraction;

using Cirrious.CrossCore;
using Cirrious.CrossCore.Platform;
using Cirrious.MvvmCross.Plugins.Messenger;
using Cirrious.MvvmCross.ViewModels;

using Xamarin;

namespace bstrkr.mvvm.viewmodels
{
	public class MapViewModel : BusTrackerViewModelBase
	{
		private const string UpdateVehicleLocationsTaskId = "UpdateVehicleLocations";
		private const double MaxDistanceFromBusStop = 500.0;

		private readonly ZoomToVisibleRegionExpandRatioConverter _zoomToVisibleRegionExpandRatioConverter = new ZoomToVisibleRegionExpandRatioConverter();
		private readonly ZoomToMarkerSizeConverter _zoomToMarkerSizeConverter = new ZoomToMarkerSizeConverter();

		private readonly IBusTrackerLocationService _locationService;
		private readonly ILiveDataProviderFactory _providerFactory;
		private readonly IMvxMessenger _messenger;
		private readonly IConfigManager _configManager;
		private readonly MvxSubscriptionToken _vehicleInfoSubscriptionToken;
		private readonly MvxSubscriptionToken _routeStopInfoSubscriptionToken;
		private readonly MvxSubscriptionToken _updateVehicleLocationsSubscriptionToken;
		private readonly BusTrackerConfig _config;

		private readonly IDictionary<string, VehicleViewModel> _vehicleIdToVM = new Dictionary<string, VehicleViewModel>();
		private readonly ObservableCollection<VehicleViewModel> _vehicles = new ObservableCollection<VehicleViewModel>();
		private readonly ObservableCollection<VehicleViewModel> _visibleVehicles = new ObservableCollection<VehicleViewModel>();
		private readonly ObservableCollection<RouteStopMapViewModel> _stops = new ObservableCollection<RouteStopMapViewModel>();

		private ReadOnlyObservableCollection<VehicleViewModel> _vehiclesReadOnly;
		private ReadOnlyObservableCollection<VehicleViewModel> _visibleVehiclesReadOnly;

		private MapMarkerSizes _markerSize = MapMarkerSizes.Medium;
		private ILiveDataProvider _liveDataProvider;
		private GeoPoint _mapCenter = GeoPoint.Empty;
		private GeoRect _visibleRegion;
		private bool _detectedArea = false;
		private float _zoom;
		private RouteStop _routeStop;
		private VehicleViewModel _selectedVehicle;
		private RouteStopMapViewModel _selectedRouteStop;
		private ISubject<VehiclesViewPortUpdate> _viewPortUpdateSubject;
		private IDisposable _subscription;

		public MapViewModel(
					IBusTrackerLocationService locationService,
					ILiveDataProviderFactory providerFactory,
					IConfigManager configManager,
					IMvxMessenger messenger)
		{
			_vehiclesReadOnly = new ReadOnlyObservableCollection<VehicleViewModel>(_vehicles);
			_visibleVehiclesReadOnly = new ReadOnlyObservableCollection<VehicleViewModel>(_visibleVehicles);

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
			this.SelectVehicleCommand = new MvxCommand<string>(this.SelectVehicle);
			this.ClearSelectionCommand = new MvxCommand(this.ClearSelection);

			_vehicleInfoSubscriptionToken = _messenger.Subscribe<ShowVehicleForecastOnMapMessage>(
													message => this.SelectVehicleCommand.Execute(message.VehicleId));

			_routeStopInfoSubscriptionToken = _messenger.Subscribe<ShowRouteStopForecastOnMapMessage>(
													message => this.SelectRouteStopCommand.Execute(message.RouteStopId));

			_updateVehicleLocationsSubscriptionToken = _messenger.Subscribe<VehicleLocationsUpdateRequestMessage>(
													message => this.ForceVehicleLocationsUpdate());

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


			_viewPortUpdateSubject = new Subject<VehiclesViewPortUpdate>();
			_subscription = _viewPortUpdateSubject.Throttle(TimeSpan.FromMilliseconds(200))
								  				  .Subscribe(this.UpdateVehiclesInTheViewPort);
		}

		public MvxCommand<string> SelectRouteStopCommand { get; private set; }

		public MvxCommand<string> SelectVehicleCommand { get; private set; }

		public MvxCommand ClearSelectionCommand { get; private set; }

		public MvxCommand<Tuple<GeoPoint, bool>> UpdateMapCenterCommand { get; private set; }

		public ReadOnlyObservableCollection<VehicleViewModel> Vehicles { get { return _vehiclesReadOnly; } }

		public ReadOnlyObservableCollection<VehicleViewModel> VisibleVehicles { get { return _visibleVehiclesReadOnly; } }

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
					_viewPortUpdateSubject.OnNext(this.GenerateUpdateFrom(this.VisibleRegion, this.Zoom));
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
			if (_liveDataProvider != null)
			{
				_liveDataProvider.Start();
			}
		}

		public void Stop()
		{
			if (_liveDataProvider != null)
			{
				_liveDataProvider.Stop();
			}
		}

		private void ChangeArea(Area area, GeoPoint centerPosition)
		{
			MvxTrace.Trace("Area changed to {0}", area != null ? area.Name : string.Empty);

			this.MapCenter = centerPosition;
			if (_liveDataProvider != null)
			{
				_liveDataProvider.Stop();
				_liveDataProvider.VehicleLocationsUpdateStarted -= this.OnVehicleLocationsUpdateStarted;
				_liveDataProvider.VehicleLocationsUpdateFailed -= this.OnVehicleLocationsUpdateFailed;
				_liveDataProvider.VehicleLocationsUpdated -= this.OnVehicleLocationsUpdated;
			}

			_stops.Clear();
			lock(_vehicles)
			{
				_vehicleIdToVM.Clear();
				_vehicles.Clear();
				_visibleVehicles.Clear();
			}

			this.InitializeStartLiveDataProvider(_providerFactory);

			this.IsBusy = false;
		}

		private void InitializeStartLiveDataProvider(ILiveDataProviderFactory factory)
		{
			_liveDataProvider = factory.GetCurrentProvider();
			if (_liveDataProvider != null)
			{
				_liveDataProvider.VehicleLocationsUpdateStarted += this.OnVehicleLocationsUpdateStarted;
				_liveDataProvider.VehicleLocationsUpdateFailed += this.OnVehicleLocationsUpdateFailed;
				_liveDataProvider.VehicleLocationsUpdated += this.OnVehicleLocationsUpdated;
//				_liveDataProvider.RouteStopForecastReceived += this.OnRouteStopForecastReceived;
//				_liveDataProvider.VehicleForecastReceived += this.OnVehicleForecastReceived;
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
					lock(_stops)
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

		private void OnVehicleLocationsUpdated(object sender, VehicleLocationsUpdatedEventArgs args)
		{
			try
			{
				MvxTrace.Trace("vehicle locations received, count={0}", args.VehicleLocations.Count);
				_messenger.Publish(new BackgroundTaskStateChangedMessage(this, UpdateVehicleLocationsTaskId, BackgroundTaskState.Finished));

				_viewPortUpdateSubject.OnNext(this.GenerateUpdateFrom(args.VehicleLocations, this.VisibleRegion, this.Zoom));
			} 
			catch (Exception e)
			{
				Insights.Report(e, Xamarin.Insights.Severity.Warning);
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
				Insights.Report(e, Insights.Severity.Warning);
				return null;
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

		private void UpdateVehicleVM(VehicleViewModel vehicleVM, VehicleLocationUpdate locationUpdate)
		{
			vehicleVM.AnimateMovement = this.IsAnimationEnabled(this.Zoom);
			vehicleVM.Update(locationUpdate);
		}

		private void SelectRouteStop(string routeStopId)
		{
			if (string.IsNullOrEmpty(routeStopId))
			{
				this.ClearSelection();
				return;
			}

			RouteStopMapViewModel routeStopVM;
			lock(_stops)
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
				this.ClearSelection();
				return;
			}

			VehicleViewModel vehicleVM;
			lock(_vehicles)
			{
				vehicleVM = _vehicles.FirstOrDefault(x => x.Model.Id.Equals(vehicleId));
			}

			if (vehicleVM == null)
			{
				return;
			}

			this.ClearSelection();

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

			this.CenterMap(_selectedVehicle.Location.Position);
		}

		private void OnZoomChanged(float zoom)
		{
			_markerSize = (MapMarkerSizes)_zoomToMarkerSizeConverter.Convert(
																		  zoom,
																		  typeof(MapMarkerSizes),
																		  null,
																		  CultureInfo.InvariantCulture);
			lock(_vehicles)
			{
				foreach (var vm in _vehicles)
				{
					vm.MarkerSize = _markerSize;
					vm.AnimateMovement = this.IsAnimationEnabled(zoom);
					vm.IsTitleVisible = this.IsVehicleTitleMarkerVisible(zoom);
				}
			}

			lock(_stops)
			{
				foreach (var routeStop in _stops)
				{
					routeStop.IsVisible = this.IsRouteStopVisible(zoom);
				}
			}
		}

		private bool IsAnimationEnabled(float zoom)
		{
			return Settings.AnimateMarkers && zoom > _config.AnimateMarkersMovementZoomThreshold;
		}

		private bool IsVehicleTitleMarkerVisible(float zoom)
		{
			return zoom > _config.ShowVehicleTitlesZoomThreshold;
		}

		private bool IsRouteStopVisible(float zoom)
		{
			return zoom > _config.ShowRouteStopsZoomThreshold;
		}

		private void SetRouteStopMarkersSelectionState(
									MapMarkerSelectionStates selectionState,
									IEnumerable<string> excludeStops = null)
		{
			lock(_stops)
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

		private void SetVehicleMarkersSelectionState(
									MapMarkerSelectionStates selectionState,
									IEnumerable<string> excludeVehicles = null)
		{
			lock(_vehicles)
			{
				foreach (var vehicle in _vehicles)
				{
					if (excludeVehicles != null)
					{
						if (!excludeVehicles.Any(v => v.Equals(vehicle.VehicleId)))
						{
							vehicle.SelectionState = selectionState;
						}
					}
					else
					{
						vehicle.SelectionState = selectionState;
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
			this.SetVehicleMarkersSelectionState(selectionState, excludeVehicles);
		}

		private void ClearSelection()
		{
			this.SetMarkersSelectionState(MapMarkerSelectionStates.NoSelection);
		}

		private void CenterMap(GeoPoint location)
		{
			this.MapCenter = location;
		}

		private void OnVehicleLocationsUpdateStarted(object sender, EventArgs EventArgs)
		{
			_messenger.Publish(new BackgroundTaskStateChangedMessage(this, UpdateVehicleLocationsTaskId, BackgroundTaskState.Running));
		}

		private void OnVehicleLocationsUpdateFailed(object sender, EventArgs EventArgs)
		{
			_messenger.Publish(new BackgroundTaskStateChangedMessage(this, UpdateVehicleLocationsTaskId, BackgroundTaskState.Failed));
		}

		private void ForceVehicleLocationsUpdate()
		{
			var provider = _liveDataProvider;
			if (provider != null)
			{
				provider.UpdateVehicleLocationsAsync();
			};
		}

		private void UpdateVehiclesInTheViewPort(VehiclesViewPortUpdate update)
		{
			MvxTrace.Trace("Updating vehicles in the viewport... ");

			try
			{
				foreach (var vehicleUpdate in update.VehicleUpdates) 
				{
					if (_vehicleIdToVM.ContainsKey(vehicleUpdate.Vehicle.Id))
					{
						this.UpdateVehicleVM(_vehicleIdToVM[vehicleUpdate.Vehicle.Id], vehicleUpdate);
					}
					else
					{
						var vm = this.CreateVehicleVM(vehicleUpdate);
						if (vm != null)
						{
							_vehicles.Add(vm);
							_vehicleIdToVM[vehicleUpdate.Vehicle.Id] = this.CreateVehicleVM(vehicleUpdate);
						}
					}
				}

				var visibleVehicles = new Dictionary<string, VehicleViewModel>();
				foreach (var keyValuePair in _vehicleIdToVM)
				{
					if (update.VisibleRegion.ContainsPoint(keyValuePair.Value.LocationAnimated))
					{
						visibleVehicles.Add(keyValuePair.Key, keyValuePair.Value);
					}
				}

				MvxTrace.Trace("{0} visible vehicles, {1} current vehicles are visible", visibleVehicles.Count, this.Vehicles.Count);

				var vehiclesToRemove = new List<VehicleViewModel>();
				foreach (var vehicle in this.VisibleVehicles)
				{
					if (!visibleVehicles.ContainsKey(vehicle.VehicleId))
					{
						vehiclesToRemove.Add(vehicle);
					}

					visibleVehicles.Remove(vehicle.VehicleId);
				}

				var animateMovement = this.IsAnimationEnabled(update.Zoom);
				this.ViewDispatcher.RequestMainThreadAction(() =>
				{
					MvxTrace.Trace("{0} vehicles should be removed, {1} added", vehiclesToRemove.Count, visibleVehicles.Count);

					foreach (var vehicle in vehiclesToRemove)
					{
						vehicle.IsInView = false;
						vehicle.AnimateMovement = false;
						_visibleVehicles.Remove(vehicle);
					}

					foreach (var vehicle in visibleVehicles.Values)
					{
						vehicle.IsInView = true;
						vehicle.AnimateMovement = animateMovement;
						_visibleVehicles.Add(vehicle);
					}
				});
			} 
			catch (Exception ex)
			{
				Insights.Report(ex, Insights.Severity.Error);
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

		private class VehiclesViewPortUpdate
		{
			public IEnumerable<VehicleLocationUpdate> VehicleUpdates { get; set; }

			public GeoRect VisibleRegion { get; set; }

			public float Zoom { get; set; }
		}
	}
}