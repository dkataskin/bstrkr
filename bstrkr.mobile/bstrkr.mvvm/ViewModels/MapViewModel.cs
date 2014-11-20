using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;

using bstrkr.core;
using bstrkr.core.config;
using bstrkr.core.map;
using bstrkr.core.services.location;
using bstrkr.core.spatial;
using bstrkr.providers;
using bstrkr.mvvm.converters;
using System.Linq;

namespace bstrkr.mvvm.viewmodels
{
	public class MapViewModel : BusTrackerViewModelBase
	{
		private readonly ILocationService _locationService;
		private readonly IConfigManager _configManager;

		private readonly ObservableCollection<VehicleViewModel> _vehicles = new ObservableCollection<VehicleViewModel>();
		private readonly ObservableCollection<RouteStopViewModel> _stops = new ObservableCollection<RouteStopViewModel>();
		private readonly ZoomToMarkerSizeConverter _zoomConverter = new ZoomToMarkerSizeConverter();

		private MapMarkerSizes _markerSize = MapMarkerSizes.Small;
		private ILiveDataProvider _liveDataProvider;
		private GeoPoint _location = GeoPoint.Empty;
		private RouteStop _routeStop;
		private Area _coarseLocation;

		public MapViewModel(IConfigManager configManager, ILocationService locationService)
		{
			_locationService = locationService;
			_locationService.LocationUpdated += OnLocationUpdated;

			_configManager = configManager;

			this.Vehicles = new ReadOnlyObservableCollection<VehicleViewModel>(_vehicles);
			this.Stops = new ReadOnlyObservableCollection<RouteStopViewModel>(_stops);

			// android requires location watcher to be started on the UI thread
			this.Dispatcher.RequestMainThreadAction(() => _locationService.StartUpdating());
		}

		public ReadOnlyObservableCollection<VehicleViewModel> Vehicles { get; private set; }

		public ReadOnlyObservableCollection<RouteStopViewModel> Stops { get; private set; }

		public GeoPoint Location 
		{ 
			get 
			{
				return _location;
			}

			private set 
			{
				if (!_location.Equals(value))
				{
					_location = value;
					this.RaisePropertyChanged(() => this.Location);
				}
			}
		}

		public Area CoarseLocation
		{
			get 
			{ 
				return _coarseLocation; 
			}

			private set 
			{
				if (_coarseLocation != value)
				{
					_coarseLocation = value;
					this.RaisePropertyChanged(() => this.CoarseLocation);
				}
			}
		}

		public RouteStop RouteStop
		{
			get 
			{
				return _routeStop;
			}

			private set
			{
				if (_routeStop != value)
				{
					_routeStop = value;
					this.RaisePropertyChanged(() => this.RouteStop);
				}
			}
		}

		public MapMarkerSizes MarkerSize
		{
			get
			{
				return _markerSize;
			}

			set
			{
				if (_markerSize != value)
				{
					_markerSize = value;
					this.RaisePropertyChanged(() => this.MarkerSize);
					this.OnMarkerSizeChanged(value);
				}
			}
		}

		private void OnLocationUpdated(object sender, LocationUpdatedEventArgs args)
		{
			this.Location = args.Location;

			this.SelectLiveDataProvider();
		}

		private void SelectLiveDataProvider()
		{
			if (_liveDataProvider == null && !this.Location.Equals(GeoPoint.Empty))
			{
				var config = _configManager.GetConfig();

				var location = config.Areas
					.Select(x => new Tuple<double, Area>(
						this.Location.DistanceTo(new GeoPoint(x.Latitude, x.Longitude)), 
						x))
					.OrderBy(x => x.Item1)
					.First();

				if (location.Item1 <= AppConsts.MaxDistanceFromCityCenter)
				{
					this.CoarseLocation = location.Item2;

					_liveDataProvider = new Bus13LiveDataProvider(
						location.Item2.Endpoint, 
						location.Item2.Id,
						TimeSpan.FromMilliseconds(UpdateInterval));
					_liveDataProvider.VehicleLocationsUpdated += this.OnVehicleLocationsUpdated;
					_liveDataProvider.Start();

					Task.Factory.StartNew(async () => await this.LoadRouteStopsAsync());
				}
				else
				{
					this.OnLocationUnknown();
				}
			}
		}

		private async Task LoadRouteStopsAsync()
		{
			var stops = await _liveDataProvider.GetRouteStopsAsync();
			lock(_stops)
			{
				this.Dispatcher.RequestMainThreadAction(() =>
				{
					foreach (var stop in stops)
					{
						var vm = this.CreateRouteStopVM(stop);
						_stops.Add(vm);
					}
				});

				this.SelectClosestRouteStop(this.Location);
			};
		}

		private void SelectClosestRouteStop(GeoPoint location)
		{
			var closestStop = _stops.Select(x => x.Model)
				.Select(x => new Tuple<double, RouteStop>(location.DistanceTo(x.Location), x))
				.OrderBy(x => x.Item1)
				.First();

			if (closestStop.Item1 <= AppConsts.MaxDistanceFromBusStop)
			{
				this.Dispatcher.RequestMainThreadAction(() => this.RouteStop = closestStop.Item2);
			}
		}

		private void OnVehicleLocationsUpdated(object sender, VehicleLocationsUpdatedEventArgs args)
		{
			try
			{
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
				Debug.WriteLine(e.ToString());
			}
		}

		private VehicleViewModel CreateVehicleVM(VehicleLocationUpdate locationUpdate)
		{
			var vehicleVM = Mvx.IocConstruct<VehicleViewModel>();
			vehicleVM.Model = locationUpdate.Vehicle;
			vehicleVM.MarkerSize = this.MarkerSize;

			return vehicleVM;
		}

		private RouteStopViewModel CreateRouteStopVM(RouteStop routeStop)
		{
			var stopVM = Mvx.IocConstruct<RouteStopViewModel>();
			stopVM.Model = routeStop;
			stopVM.MarkerSize = this.MarkerSize;

			return stopVM;
		}

		private void UpdateVehicleVM(VehicleViewModel vehicleVM, VehicleLocationUpdate locationUpdate)
		{
			vehicleVM.UpdateLocation(locationUpdate.Vehicle.Location, locationUpdate.Waypoints);
			vehicleVM.VehicleHeading = locationUpdate.Vehicle.Heading;
		}

		private void OnLocationUnknown()
		{
		}

		private void OnMarkerSizeChanged(MapMarkerSizes size)
		{
			lock(_vehicles)
			{
				foreach (var vm in _vehicles)
				{
					vm.MarkerSize = size;
				}
			}

			lock(_stops)
			{
				foreach (var vm in _stops)
				{
					vm.MarkerSize = size;
				}
			}
		}
	}
}