using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;

using bstrkr.core;
using bstrkr.core.collections;
using bstrkr.core.config;
using bstrkr.core.map;
using bstrkr.core.services.location;
using bstrkr.core.spatial;
using bstrkr.providers;
using bstrkr.mvvm.converters;
using Chance.MvvmCross.Plugins.UserInteraction;

namespace bstrkr.mvvm.viewmodels
{
	public class MapViewModel : BusTrackerViewModelBase
	{
		private readonly IBusTrackerLocationService _locationService;
		private readonly ILiveDataProviderFactory _providerFactory;

		private readonly ObservableCollection<VehicleViewModel> _vehicles = new ObservableCollection<VehicleViewModel>();
		private readonly ObservableCollection<RouteStopViewModel> _stops = new ObservableCollection<RouteStopViewModel>();
		private readonly ZoomToMarkerSizeConverter _zoomConverter = new ZoomToMarkerSizeConverter();

		private MapMarkerSizes _markerSize = MapMarkerSizes.Small;
		private ILiveDataProvider _liveDataProvider;
		private GeoPoint _location = GeoPoint.Empty;
		private RouteStop _routeStop;
		private bool _isBusy;

		public MapViewModel(IBusTrackerLocationService locationService, ILiveDataProviderFactory providerFactory)
		{
			_providerFactory = providerFactory;

			_locationService = locationService;
			_locationService.LocationChanged += OnLocationChanged;
			_locationService.LocationError += OnLocationError;

			this.Vehicles = new ReadOnlyObservableCollection<VehicleViewModel>(_vehicles);
			this.Stops = new ReadOnlyObservableCollection<RouteStopViewModel>(_stops);

			// android requires location watcher to be started on the UI thread
			this.Dispatcher.RequestMainThreadAction(() => _locationService.Start());
			this.IsBusy = true;
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

		public bool IsBusy 
		{
			get { return _isBusy; }
			private set
			{
				if (_isBusy != value)
				{
					_isBusy = value;
					this.RaisePropertyChanged(() => this.IsBusy);
				}
			}
		}

		private void OnLocationChanged(object sender, EventArgs args)
		{
			this.Location = _locationService.Location;

			if (_liveDataProvider == null)
			{
				_liveDataProvider = _providerFactory.CreateProvider(_locationService.Area);
				if (_liveDataProvider != null)
				{
					_liveDataProvider.VehicleLocationsUpdated += this.OnVehicleLocationsUpdated;
					_liveDataProvider.Start();
				}
			}

			this.IsBusy = false;
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
//			var closestStop = _stops.Select(x => x.Model)
//				.Select(x => new Tuple<double, RouteStop>(location.DistanceTo(x.Location), x))
//				.OrderBy(x => x.Item1)
//				.First();
//
//			if (closestStop.Item1 <= AppConsts.MaxDistanceFromBusStop)
//			{
//				this.Dispatcher.RequestMainThreadAction(() => this.RouteStop = closestStop.Item2);
//			}
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

		private void OnLocationError(object sender, LocationErrorEventArgs args)
		{
			this.IsBusy = false;

			switch (args.Error)
			{
				case LocationErrors.UnknownArea:
					Mvx.Resolve<IUserInteraction>().Confirm(
									"Wrong area", 
									answer => 
									{
										this.ShowViewModel<SetAreaViewModel>();
									},
									"wrong area title",
									"No, thanks",
									"Yes");
					break;

				default:
					break;
			}
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