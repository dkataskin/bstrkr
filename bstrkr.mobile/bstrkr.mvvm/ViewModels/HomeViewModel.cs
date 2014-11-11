using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;

using bstrkr.core;
using bstrkr.core.collections;
using bstrkr.core.config;
using bstrkr.core.consts;
using bstrkr.core.interfaces;
using bstrkr.core.map;
using bstrkr.core.providers.bus13;
using bstrkr.core.services.location;
using bstrkr.core.spatial;
using bstrkr.mvvm.converters;
using bstrkr.mvvm.views;

namespace bstrkr.mvvm.viewmodels
{
	public class HomeViewModel : BusTrackerViewModelBase
    {
		private const int UpdateInterval = 1000;

		private readonly ILocationService _locationService;
		private readonly IConfigManager _configManager;
		private readonly ZoomToMarkerSizeConverter _zoomConverter = new ZoomToMarkerSizeConverter();
		private readonly ObservableCollection<VehicleViewModel> _vehicles = new ObservableCollection<VehicleViewModel>();
		private readonly ObservableCollection<RouteStopViewModel> _stops = new ObservableCollection<RouteStopViewModel>();

		private MapMarkerSizes _markerSize = MapMarkerSizes.Small;
		private ILiveDataProvider _liveDataProvider;
		private GeoPoint _location = GeoPoint.Empty;
		private RouteStop _routeStop;
		private BusTrackerLocation _coarseLocation;

		public HomeViewModel(IConfigManager configManager, ILocationService locationService)
		{
			this.MenuItems = new ReadOnlyObservableCollection<MenuViewModel>(this.CreateMenuViewModels());

			this.Vehicles = new ReadOnlyObservableCollection<VehicleViewModel>(_vehicles);
			this.Stops = new ReadOnlyObservableCollection<RouteStopViewModel>(_stops);

			_configManager = configManager;

			_locationService = locationService;
			_locationService.LocationUpdated += OnLocationUpdated;

			this.SelectMenuItemCommand = new MvxCommand<MenuViewModel>(this.SelectMenuItem);

			// android requires location watcher to be started on the UI thread
			this.Dispatcher.RequestMainThreadAction(() => _locationService.StartUpdating());
		}

		public ReadOnlyObservableCollection<MenuViewModel> MenuItems { get; private set; }

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

		public BusTrackerLocation CoarseLocation
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

		public ICommand SelectMenuItemCommand { get; private set; }

		public MenuSection GetSectionForViewModelType(Type type)
		{
			if (type == typeof(MapViewModel))
			{
				return MenuSection.Map;
			}

			if (type == typeof(RoutesViewModel))
			{
				return MenuSection.Routes;
			}

			if (type == typeof(SettingsViewModel))
			{
				return MenuSection.Settings;
			}

			if (type == typeof(AboutViewModel))
			{
				return MenuSection.About;
			}

			return MenuSection.Unknown;
		}

		private ObservableCollection<MenuViewModel> CreateMenuViewModels()
		{
			return new ObservableCollection<MenuViewModel> 
			{
				new MenuViewModel { Section = MenuSection.Map },
				new MenuViewModel { Section = MenuSection.Routes },
				new MenuViewModel { Section = MenuSection.Settings },
				new MenuViewModel { Section = MenuSection.About }
			};
		}

		private void SelectMenuItem(MenuViewModel item)
		{
			switch (item.Section)
			{
				case MenuSection.Map:
					this.ShowViewModel<MapViewModel>(new { item.Id });
					break;

				case MenuSection.Routes:
					this.ShowViewModel<RoutesViewModel>(new { item.Id });
					break;

				case MenuSection.Settings:
					this.ShowViewModel<SettingsViewModel>(new { item.Id });
					break;


				case MenuSection.About:
					this.ShowViewModel<AboutViewModel>(new { item.Id });
					break;
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

				var location = config.Locations
									 .Select(x => new Tuple<double, BusTrackerLocation>(
																				this.Location.DistanceTo(new GeoPoint(x.Latitude, x.Longitude)), 
																				x))
									 .OrderBy(x => x.Item1)
									 .First();

				if (location.Item1 <= AppConsts.MaxDistanceFromCityCenter)
				{
					this.CoarseLocation = location.Item2;

					_liveDataProvider = new Bus13LiveDataProvider(
														location.Item2.Endpoint, 
														location.Item2.LocationId,
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