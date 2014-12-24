using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Chance.MvvmCross.Plugins.UserInteraction;

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
using Cirrious.CrossCore.Platform;
using Xamarin;

namespace bstrkr.mvvm.viewmodels
{
	public class MapViewModel : BusTrackerViewModelBase
	{
		private readonly IBusTrackerLocationService _locationService;
		private readonly ILiveDataProviderFactory _providerFactory;

		private readonly ObservableCollection<VehicleViewModel> _vehicles = new ObservableCollection<VehicleViewModel>();
		private readonly ObservableCollection<RouteStopMapViewModel> _stops = new ObservableCollection<RouteStopMapViewModel>();
		private readonly ZoomToMarkerSizeConverter _zoomConverter = new ZoomToMarkerSizeConverter();

		private MapMarkerSizes _markerSize = MapMarkerSizes.Medium;
		private ILiveDataProvider _liveDataProvider;
		private GeoPoint _location = GeoPoint.Empty;
		private RouteStop _routeStop;

		public MapViewModel(IBusTrackerLocationService locationService, ILiveDataProviderFactory providerFactory)
		{
			_providerFactory = providerFactory;

			_locationService = locationService;
			_locationService.LocationChanged += OnLocationChanged;
			_locationService.LocationError += OnLocationError;

			this.Vehicles = new ReadOnlyObservableCollection<VehicleViewModel>(_vehicles);
			this.Stops = new ReadOnlyObservableCollection<RouteStopMapViewModel>(_stops);

			// android requires location watcher to be started on the UI thread
			this.Dispatcher.RequestMainThreadAction(() => _locationService.Start());
			this.IsBusy = true;
		}

		public ReadOnlyObservableCollection<VehicleViewModel> Vehicles { get; private set; }

		public ReadOnlyObservableCollection<RouteStopMapViewModel> Stops { get; private set; }

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

		private void OnLocationChanged(object sender, EventArgs args)
		{
			MvxTrace.Trace(MvxTraceLevel.Diagnostic, () => "Location changed");

			this.Location = _locationService.Location;

			if (_liveDataProvider == null)
			{
				_liveDataProvider = _providerFactory.CreateProvider(_locationService.Area);
				if (_liveDataProvider != null)
				{
					_liveDataProvider.VehicleLocationsUpdated += this.OnVehicleLocationsUpdated;
					_liveDataProvider.Start();

					MvxTrace.Trace(() => "provider started");
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
				Insights.Report(e, ReportSeverity.Warning);
			}
		}

		private VehicleViewModel CreateVehicleVM(VehicleLocationUpdate locationUpdate)
		{
			var vehicleVM = Mvx.IocConstruct<VehicleViewModel>();
			vehicleVM.Model = locationUpdate.Vehicle;
			vehicleVM.MarkerSize = this.MarkerSize;

			return vehicleVM;
		}

		private RouteStopMapViewModel CreateRouteStopVM(RouteStop routeStop)
		{
			var stopVM = Mvx.IocConstruct<RouteStopMapViewModel>();
			stopVM.Model = routeStop;
			stopVM.MarkerSize = this.MarkerSize;

			return stopVM;
		}

		private void UpdateVehicleVM(VehicleViewModel vehicleVM, VehicleLocationUpdate locationUpdate)
		{
			vehicleVM.UpdateLocation(locationUpdate.Vehicle.Location, locationUpdate.Waypoints);
			vehicleVM.VehicleHeading = locationUpdate.Vehicle.Heading;
		}

		private void OnLocationError(object sender, BusTrackerLocationErrorEventArgs args)
		{
			this.IsBusy = false;

			switch (args.Error)
			{
				case BusTrackerLocationErrors.UnknownArea:
					Mvx.Resolve<IUserInteraction>().Confirm(
									AppResources.unknown_location_dialog_text, 
									answer => 
									{
										if (answer)
										{
											this.ShowViewModel<SetAreaViewModel>();
										}
									},
									AppResources.unknown_location_dialog_title,
									AppResources.yes,
									AppResources.no_thanks);
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