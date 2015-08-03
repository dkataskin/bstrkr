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
		private readonly BusTrackerConfig _config;

		private readonly ObservableCollection<VehicleViewModel> _vehicles = new ObservableCollection<VehicleViewModel>();
		private readonly ObservableCollection<RouteStopMapViewModel> _stops = new ObservableCollection<RouteStopMapViewModel>();

		private ReadOnlyObservableCollection<VehicleViewModel> _vehiclesReadOnly = 
			new ReadOnlyObservableCollection<VehicleViewModel>(new ObservableCollection<VehicleViewModel>());

		private MapMarkerSizes _markerSize = MapMarkerSizes.Medium;
		private ILiveDataProvider _liveDataProvider;
		private GeoPoint _location = GeoPoint.Empty;
		private GeoRect _visibleRegion;
		private bool _detectedArea = false;
		private float _zoom;
		private RouteStop _routeStop;
		private VehicleViewModel _selectedVehicle;

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
			this.ShowRouteStopInfoCommand = new MvxCommand<RouteStopMapViewModel>(this.ShowRouteStopInfo, vm => vm != null);
			this.ShowVehicleInfoCommand = new MvxCommand<string>(this.ShowVehicleInfo, vm => vm != null);

			_vehicleInfoSubscriptionToken = _messenger.Subscribe<ShowVehicleForecastOnMapMessage>(
													message => this.ShowVehicleInfoCommand.Execute(message.VehicleId));
		}

		public MvxCommand<RouteStopMapViewModel> ShowRouteStopInfoCommand { get; private set; }

		public MvxCommand<string> ShowVehicleInfoCommand { get; private set; }

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

		public GeoPoint Location 
		{ 
			get { return _location; }
			private set 
			{
				if (!_location.Equals(value))
				{
					_location = value;
					this.RaisePropertyChanged(() => this.Location);
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

		private void OnLocationChanged(object sender, EventArgs args)
		{
			MvxTrace.Trace(MvxTraceLevel.Diagnostic, () => "Location changed");

			this.Location = _locationService.Location;
			this.DetectedArea = _locationService.DetectedArea;

			if (_liveDataProvider != null)
			{
				_liveDataProvider.Stop();
				_liveDataProvider.VehicleLocationsUpdated -= this.OnVehicleLocationsUpdated;
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

						this.SelectClosestRouteStop(this.Location);
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
				this.Dispatcher.RequestMainThreadAction(() => this.ShowRouteStopInfo(
																			closestRouteStop.Id,
																			closestRouteStop.Name,
																			closestRouteStop.Description));
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

			return vehicleVM;
		}

		private RouteStopMapViewModel CreateRouteStopVM(RouteStop routeStop)
		{
			var stopVM = Mvx.IocConstruct<RouteStopMapViewModel>();
			stopVM.Model = routeStop;
			stopVM.MarkerSize = MapMarkerSizes.Medium;

			return stopVM;
		}

		private void UpdateVehicleVM(VehicleViewModel vehicleVM, VehicleLocationUpdate locationUpdate)
		{
			vehicleVM.AnimateMovement = this.GetAnimateMarkerMovementFlag(this.Zoom);
			vehicleVM.UpdateLocation(locationUpdate.Vehicle.Location, locationUpdate.Waypoints);
		}

		private void ShowRouteStopInfo(RouteStopMapViewModel routeStopVM)
		{
			var requestedBy = new MvxRequestedBy(MvxRequestedByType.UserAction, "map_tap");
			this.ShowViewModel<RouteStopViewModel>(
											new 
											{
												id = routeStopVM.Model.Id,
												name = routeStopVM.Model.Name,
												description = routeStopVM.Model.Description
											}, 
											null, 
											requestedBy);
			
			this.Location = routeStopVM.Location.Position;
		}

		private void ShowRouteStopInfo(string id, string name, string description)
		{
			var requestedBy = new MvxRequestedBy(MvxRequestedByType.UserAction, "map_tap");
			this.ShowViewModel<RouteStopViewModel>(
												new 
												{
													id = id,
													name = name,
													description = description
												},
												null, 
												requestedBy);
		}

		private void ShowVehicleInfo(string vehicleId)
		{
			VehicleViewModel vehicleVM;
			lock(_vehicles)
			{
				vehicleVM = _vehicles.FirstOrDefault(x => x.Model.Id.Equals(vehicleId));
			}

			if (vehicleVM == null)
			{
				return;
			}

//			if (_selectedVehicle != null)
//			{
//				_selectedVehicle.PropertyChanged -= this.OnVehicleVMPropertyChanged;
//			}

//			_selectedVehicle = vehicleVM;
//			_selectedVehicle.PropertyChanged += this.OnVehicleVMPropertyChanged;

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
			this.Location = _selectedVehicle.Location.Position;
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

		private void OnVehicleVMPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			if (args.PropertyName.Equals("Location"))
			{
				this.Location = (sender as VehicleViewModel).Location.Position;
			}

			if (args.PropertyName.Equals("PositionAnimation"))
			{
				this.Location = (sender as VehicleViewModel).PositionAnimation;
			}
		}
	}
}