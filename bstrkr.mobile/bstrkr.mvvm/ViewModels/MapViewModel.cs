using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
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
		private const double MaxDistanceFromBusStop = 500.0;

		private readonly IBusTrackerLocationService _locationService;
		private readonly ILiveDataProviderFactory _providerFactory;
		private readonly IMvxMessenger _messenger;
		private readonly IConfigManager _configManager;
		private readonly MvxSubscriptionToken _vehicleInfoSubscriptionToken;
		private readonly MvxSubscriptionToken _routeStopInfoSubscriptionToken;
		private readonly BusTrackerConfig _config;

		private readonly ObservableCollection<VehicleViewModel> _vehicles = new ObservableCollection<VehicleViewModel>();
		private readonly ObservableCollection<RouteStopMapViewModel> _stops = new ObservableCollection<RouteStopMapViewModel>();

		private ReadOnlyObservableCollection<VehicleViewModel> _vehiclesReadOnly = 
			new ReadOnlyObservableCollection<VehicleViewModel>(new ObservableCollection<VehicleViewModel>());

		private MapMarkerSizes _markerSize = MapMarkerSizes.Medium;
		private ILiveDataProvider _liveDataProvider;
		private GeoPoint _mapCenter = GeoPoint.Empty;
		private GeoRect _visibleRegion;
		private bool _detectedArea = false;
		private float _zoom;
		private float _viewportOffset = 1.0f;
		private RouteStop _routeStop;
		private VehicleViewModel _selectedVehicle;
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
			_locationService.LocationChanged += OnLocationChanged;

			_config = _configManager.GetConfig();

			this.Stops = new ReadOnlyObservableCollection<RouteStopMapViewModel>(_stops);
			this.SelectRouteStopCommand = new MvxCommand<string>(this.SelectRouteStop);
			this.SelectVehicleCommand = new MvxCommand<string>(this.SelectVehicle);
			this.ClearSelectionCommand = new MvxCommand(this.ClearSelection);

			_vehicleInfoSubscriptionToken = _messenger.Subscribe<ShowVehicleForecastOnMapMessage>(
													message => this.SelectVehicleCommand.Execute(message.VehicleId));

			_routeStopInfoSubscriptionToken = _messenger.Subscribe<ShowRouteStopForecastOnMapMessage>(
													message => this.SelectRouteStopCommand.Execute(message.RouteStopId));

			this.ChangeMapViewportCommand = new MvxCommand<float>(this.ChangeMapViewport);
			this.MoveMapCenterCommand = new MvxCommand<GeoPoint>(newMapCenter => _mapCenter = newMapCenter);
		}

		public MvxCommand<string> SelectRouteStopCommand { get; private set; }

		public MvxCommand<string> SelectVehicleCommand { get; private set; }

		public MvxCommand ClearSelectionCommand { get; private set; }

		public MvxCommand<float> ChangeMapViewportCommand { get; private set; }

		public MvxCommand<GeoPoint> MoveMapCenterCommand { get; private set; }

		public ReadOnlyObservableCollection<VehicleViewModel> Vehicles 
		{ 
			get { return _vehiclesReadOnly; } 
			private set
			{
				_vehiclesReadOnly = value;
				this.RaisePropertyChanged(() => this.Vehicles);
			}
		}

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
			set { this.RaiseAndSetIfChanged(ref _visibleRegion, value, () => this.VisibleRegion); }
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

			if (_locationService.Area != null)
			{
				this.OnLocationChanged(this, EventArgs.Empty);
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

		private void ChangeMapViewport(float viewportOffset)
		{
			var diff =  (viewportOffset - _viewportOffset) * (VisibleRegion.NorthEast.Latitude - VisibleRegion.SouthWest.Latitude);
			this.MapCenter = new GeoPoint(this.MapCenter.Latitude + diff / 2.0f, this.MapCenter.Longitude);

			_viewportOffset = viewportOffset;
		}

		private void OnLocationChanged(object sender, EventArgs args)
		{
			MvxTrace.Trace(MvxTraceLevel.Diagnostic, () => "Location changed");

			this.MapCenter = _locationService.Location;
			this.DetectedArea = _locationService.DetectedArea;

			if (_liveDataProvider != null)
			{
				_liveDataProvider.Stop();
				_liveDataProvider.VehicleLocationsUpdated -= this.OnVehicleLocationsUpdated;
				_liveDataProvider.RouteStopForecastReceived -= this.OnRouteStopForecastReceived;
				_liveDataProvider.VehicleForecastReceived -= this.OnVehicleForecastReceived;
			}

			_stops.Clear();
			lock(_vehicles)
			{
				_vehicles.Clear();
			}

			this.InitializeStartLiveDataProvider(_providerFactory);

			this.IsBusy = false;
		}

		private void InitializeStartLiveDataProvider(ILiveDataProviderFactory factory)
		{
			_liveDataProvider = factory.GetCurrentProvider();
			if (_liveDataProvider != null)
			{
				_liveDataProvider.VehicleLocationsUpdated += this.OnVehicleLocationsUpdated;
				_liveDataProvider.RouteStopForecastReceived += this.OnRouteStopForecastReceived;
				_liveDataProvider.VehicleForecastReceived += this.OnVehicleForecastReceived;
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

					lock(_vehicles)
					{
						this.Vehicles = new ReadOnlyObservableCollection<VehicleViewModel>(_vehicles);
					}
				});
			}
			else
			{
				this.Dispatcher.RequestMainThreadAction(() =>
				{
					lock(_vehicles)
					{
						this.Vehicles = new ReadOnlyObservableCollection<VehicleViewModel>(_vehicles);
					}
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

				lock(_vehicles)
				{
					this.Dispatcher.RequestMainThreadAction(() =>
						_vehicles.Merge(
							args.VehicleLocations, 
							vehicleVM => vehicleVM.VehicleId,
							update => update.Vehicle.Id, 
							this.CreateVehicleVM,
							this.UpdateVehicleVM,
							MergeMode.Update));
				}

			} 
			catch (Exception e)
			{
				Insights.Report(e, Xamarin.Insights.Severity.Warning);
			}
		}

		private VehicleViewModel CreateVehicleVM(VehicleLocationUpdate locationUpdate)
		{
			var vehicleVM = Mvx.IocConstruct<VehicleViewModel>();
			vehicleVM.Model = locationUpdate.Vehicle;
			vehicleVM.MarkerSize = _markerSize;

			if (_selectedRouteStop != null || _selectedVehicle != null)
			{
				vehicleVM.SelectionState = MapMarkerSelectionStates.SelectionNotSelected;
			}

			return vehicleVM;
		}

		private RouteStopMapViewModel CreateRouteStopVM(RouteStop routeStop)
		{
			var stopVM = Mvx.IocConstruct<RouteStopMapViewModel>();
			stopVM.Model = routeStop;
			stopVM.MarkerSize = MapMarkerSizes.Medium;

			if (_selectedRouteStop != null || _selectedVehicle != null)
			{
				stopVM.SelectionState = MapMarkerSelectionStates.SelectionNotSelected;
			}

			return stopVM;
		}

		private void UpdateVehicleVM(VehicleViewModel vehicleVM, VehicleLocationUpdate locationUpdate)
		{
			vehicleVM.AnimateMovement = this.GetAnimateMarkerMovementFlag(this.Zoom);
			vehicleVM.UpdateLocation(locationUpdate);
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

			this.SetMarkersSelectionState(MapMarkerSelectionStates.SelectionNotSelected, null, new[] { _selectedRouteStop.Model.Id });

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

			this.CenterMap(_viewportOffset, _selectedRouteStop.Location.Position);
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

			this.SetMarkersSelectionState(MapMarkerSelectionStates.SelectionNotSelected, new[] { _selectedVehicle.Model.Id }, null);

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

			this.CenterMap(_viewportOffset, _selectedVehicle.Location.Position);
		}

		private void OnZoomChanged(float zoom)
		{
			var converter = new ZoomToMarkerSizeConverter();
			var markerSize = (MapMarkerSizes)converter.Convert(zoom, typeof(MapMarkerSizes), null, null);
			if (_markerSize != markerSize)
			{
				_markerSize = markerSize;

				lock(_vehicles)
				{
					foreach (var vm in _vehicles)
					{
						vm.MarkerSize = _markerSize;
						vm.AnimateMovement = this.GetAnimateMarkerMovementFlag(zoom);
					}
				}
			}

			lock(_stops)
			{
				foreach (var routeStop in _stops)
				{
					routeStop.IsVisible = zoom > _config.ShowRouteStopsZoomThreshold;
				}
			}
		}

		private bool GetAnimateMarkerMovementFlag(float zoom)
		{
			return Settings.AnimateMarkers && zoom > _config.AnimateMarkersMovementZoomThreshold;
		}

		private void SetMarkersSelectionState(
						MapMarkerSelectionStates selectionState,
						IEnumerable<string> excludeVehicles = null,
						IEnumerable<string> excludeStops = null)
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

		private void ClearSelection()
		{
			this.SetMarkersSelectionState(MapMarkerSelectionStates.NoSelection);
		}

		private void CenterMap(float viewportOffset, GeoPoint location)
		{
			if (viewportOffset > 0 && viewportOffset < 1)
			{
				var dx = (VisibleRegion.NorthEast.Latitude - VisibleRegion.SouthWest.Latitude);
				var diff = (viewportOffset / 2.0f) * dx;
				this.MapCenter = new GeoPoint(
									location.Latitude - diff, 
									location.Longitude);
			}
			else
			{
				this.MapCenter = location;
			}
		}

		private void OnRouteStopForecastReceived(object sender, RouteStopForecastReceivedEventArgs args)
		{
			var selectedRouteStop = _selectedRouteStop;
			if (selectedRouteStop != null && selectedRouteStop.Model.Id.Equals(args.RouteStopId) &&
				args.Forecast != null && args.Forecast.Items != null && args.Forecast.Items.Any())
			{
				var vehicleIds = args.Forecast.Items.Select(x => x.VehicleId).ToList();
				lock(_vehicles)
				{
					foreach(var vehicle in _vehicles)
					{
						if (vehicleIds.Contains(vehicle.VehicleId))
						{
							vehicle.SelectionState = MapMarkerSelectionStates.SelectionSelected;
						}
						else
						{
							vehicle.SelectionState = MapMarkerSelectionStates.SelectionNotSelected;
						}
					}
				}
			}
		}

		private void OnVehicleForecastReceived(object sender, VehicleForecastReceivedEventArgs args)
		{
			var selectedVehile = _selectedVehicle;
			if (selectedVehile != null && selectedVehile.VehicleId.Equals(args.VehicleId) &&
				args.Forecast != null && args.Forecast.Items != null && args.Forecast.Items.Any())
			{
				var routeStopIds = args.Forecast.Items.Select(x => x.RouteStop.Id).ToList();
				lock(_stops)
				{
					foreach(var routeStop in _stops)
					{
						if (routeStopIds.Contains(routeStop.Model.Id))
						{
							routeStop.SelectionState = MapMarkerSelectionStates.SelectionSelected;
						}
						else
						{
							routeStop.SelectionState = MapMarkerSelectionStates.SelectionNotSelected;
						}
					}
				}
			}
		}
	}
}