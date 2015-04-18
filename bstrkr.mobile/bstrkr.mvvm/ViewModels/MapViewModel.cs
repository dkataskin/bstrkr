using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

		private readonly ObservableCollection<VehicleViewModel> _vehicles = new ObservableCollection<VehicleViewModel>();
		private readonly ObservableCollection<RouteStopMapViewModel> _stops = new ObservableCollection<RouteStopMapViewModel>();
		private readonly ZoomToMarkerSizeConverter _zoomConverter = new ZoomToMarkerSizeConverter();

		private ReadOnlyObservableCollection<VehicleViewModel> _vehiclesReadOnly = 
			new ReadOnlyObservableCollection<VehicleViewModel>(new ObservableCollection<VehicleViewModel>());

		private MapMarkerSizes _markerSize = MapMarkerSizes.Medium;
		private ILiveDataProvider _liveDataProvider;
		private GeoPoint _location = GeoPoint.Empty;
		private bool _detectedArea = false;
		private float _zoom;
		private RouteStop _routeStop;

		public MapViewModel(
					IBusTrackerLocationService locationService, 
					ILiveDataProviderFactory providerFactory,
					IMvxMessenger messenger)
		{
			_providerFactory = providerFactory;
			_messenger = messenger;
			_locationService = locationService;
			_locationService.LocationChanged += OnLocationChanged;
			_locationService.LocationError += OnLocationError;

			this.Stops = new ReadOnlyObservableCollection<RouteStopMapViewModel>(_stops);
			this.ShowRouteStopInfoCommand = new MvxCommand<RouteStopMapViewModel>(this.ShowRouteStopInfo, vm => vm != null);
			this.ShowVehicleInfoCommand = new MvxCommand<VehicleViewModel>(this.ShowVehicleInfo, vm => vm != null);
		}

		public MvxCommand<RouteStopMapViewModel> ShowRouteStopInfoCommand { get; private set; }

		public MvxCommand<VehicleViewModel> ShowVehicleInfoCommand { get; private set; }

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

			// android requires location watcher to be started on the UI thread
			this.Dispatcher.RequestMainThreadAction(() => _locationService.Start());
			this.IsBusy = true;
		}

		private void OnLocationChanged(object sender, EventArgs args)
		{
			MvxTrace.Trace(MvxTraceLevel.Diagnostic, () => "Location changed");

			this.Location = _locationService.Location;
			this.DetectedArea = _locationService.DetectedArea;

			if (_liveDataProvider == null)
			{
				_liveDataProvider = _providerFactory.GetCurrentProvider();
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

			_messenger.Publish(new LocationUpdateMessage(this, _locationService.Area));
			this.IsBusy = false;
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
				.Select(x => new Tuple<double, RouteStop>(location.DistanceTo(x.Location), x))
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
				Insights.Report(e, ReportSeverity.Warning);
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
			vehicleVM.UpdateLocation(locationUpdate.Vehicle.Location, locationUpdate.Waypoints);
			vehicleVM.VehicleHeading = locationUpdate.Vehicle.Heading;
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

		private void ShowVehicleInfo(VehicleViewModel vehicleVM)
		{
			var requestedBy = new MvxRequestedBy(MvxRequestedByType.UserAction, "map_tap");

			var navParams = new 
			{
				id = vehicleVM.Model.Id,
				carPlate = vehicleVM.Model.CarPlate,
				vehicleType = vehicleVM.VehicleType,
				routeId = vehicleVM.Model.RouteInfo.RouteId,
				routeNumber = vehicleVM.Model.RouteInfo.RouteNumber,
				routeDisplayName = vehicleVM.Model.RouteInfo.DisplayName,
				runUpdates = true
			};

			this.ShowViewModel<RouteVehicleForecastViewModel>(navParams, null, requestedBy);
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

		private void OnZoomChanged(float zoom)
		{
			var conv = new ZoomToMarkerSizeConverter();
			var markerSize = (MapMarkerSizes)conv.Convert(zoom, typeof(MapMarkerSizes), null, null);
			if (_markerSize != markerSize)
			{
				_markerSize = markerSize;

				lock(_vehicles)
				{
					foreach (var vm in _vehicles)
					{
						vm.MarkerSize = _markerSize;
					}
				}
			}

			lock(_stops)
			{
				foreach (var routeStop in _stops)
				{
					routeStop.IsVisible = zoom > 13.0f;
				}
			}
		}
	}
}