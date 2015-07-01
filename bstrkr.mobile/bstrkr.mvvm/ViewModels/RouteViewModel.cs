using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

using bstrkr.core;
using bstrkr.core.spatial;
using bstrkr.mvvm.viewmodels;
using bstrkr.providers;

using Cirrious.CrossCore;
using Cirrious.CrossCore.Platform;
using Cirrious.MvvmCross.ViewModels;

using Xamarin;

namespace bstrkr.mvvm.viewmodels
{
	public class RouteViewModel : BusTrackerViewModelBase, ICleanable
	{
		private readonly object _lockObject = new object();
		private readonly ILiveDataProviderFactory _providerFactory;
		private readonly ObservableCollection<VehicleForecastViewModel> _vehicles = new ObservableCollection<VehicleForecastViewModel>();

		private readonly IObservable<long> _intervalObservable;
		private readonly IDisposable _intervalSubscription;

		private bool _noData;
		private string _routeId;
		private string _name;
		private string _from;
		private string _to;
		private int _number;
		private int _vehicleCount;
		private VehicleTypes _vehicleType;
		private Route _route;

		public RouteViewModel(ILiveDataProviderFactory providerFactory)
		{
			_providerFactory = providerFactory;
			this.Vehicles = new ReadOnlyObservableCollection<VehicleForecastViewModel>(_vehicles);
			this.ShowRouteVehicleDetailsCommand = new MvxCommand(() => {});

			_intervalObservable = Observable.Interval(TimeSpan.FromMilliseconds(1000));
			_intervalSubscription = _intervalObservable.Subscribe(this.OnNextInterval);
		}

		public MvxCommand ShowRouteVehicleDetailsCommand { get; private set; }

		public string RouteId
		{
			get { return _routeId; }
			private set { this.RaiseAndSetIfChanged(ref _routeId, value, () => this.RouteId); }
		}

		public int Number
		{
			get { return _number; }
			private set { this.RaiseAndSetIfChanged(ref _number, value, () => this.Number); }
		}

		public string Name 
		{ 
			get { return _name; } 
			private set { this.RaiseAndSetIfChanged(ref _name, value, () => this.Name); } 
		}

		public string From
		{
			get { return _from; }
			private set { this.RaiseAndSetIfChanged(ref _from, value, () => this.From); }
		}

		public string To
		{
			get { return _to; }
			private set { this.RaiseAndSetIfChanged(ref _to, value, () => this.To); }
		}

		public bool NoData
		{
			get { return _noData; }
			private set { this.RaiseAndSetIfChanged(ref _noData, value, () => this.NoData); }
		}

		public VehicleTypes VehicleType
		{
			get { return _vehicleType; }
			private set { this.RaiseAndSetIfChanged(ref _vehicleType, value, () => this.VehicleType); }
		}

		public Route Route 
		{ 
			get { return _route; } 
			private set { this.RaiseAndSetIfChanged(ref _route, value, () => this.Route); } 
		}

		public ReadOnlyObservableCollection<VehicleForecastViewModel> Vehicles { get; private set; }

		public ReadOnlyObservableCollection<RouteStop> Stops { get; private set; }

		public void Init(
					string routeId, 
					string routeName, 
					int routeNumber, 
					string routeIds,
					string fromStop,
					string toStop,
					VehicleTypes vehicleType)
		{
			string[] ids = null;
			if (!string.IsNullOrEmpty(routeIds))
			{
				ids = routeIds.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
			}

			this.RouteId = routeId;
			this.Number = routeNumber;
			this.Name = routeName;
			this.VehicleType = vehicleType;
			this.From = fromStop;
			this.To = toStop;

			this.Route = new Route(
							routeId, 
							ids, 
							routeName, 
							string.Empty, 
							new List<RouteStop>(), 
							new List<GeoPoint>(),
							new List<VehicleTypes> { vehicleType });
		}

		public override void Start()
		{
			base.Start();

			this.IsBusy = true;

			var provider = _providerFactory.GetCurrentProvider();
			if (provider != null)
			{
				provider.GetRouteAsync(this.RouteId)
						.ContinueWith(this.SetRoute)
						.ConfigureAwait(false);
				
				provider.GetRouteVehiclesAsync(new[] { this.Route })
						.ContinueWith(this.ShowRouteVehicles)
						.ConfigureAwait(false);
			}
		}

		public void CleanUp()
		{
			if (_intervalSubscription != null)
			{
				_intervalSubscription.Dispose();
			}
		}

		private void SetRoute(Task<Route> task)
		{
			if (task.Status == TaskStatus.RanToCompletion && task.Result != null)
			{
				this.Route = task.Result;
			}
		}

		private void ShowRouteVehicles(Task<IEnumerable<Vehicle>> task)
		{
			if (task.Status != TaskStatus.RanToCompletion)
			{
				Insights.Report(task.Exception, Xamarin.Insights.Severity.Error);
				return;
			}

			var vehicles = task.Result;
			if (vehicles != null && vehicles.Any())
			{
				var vms = vehicles.Select(this.CreateFromVehicle).ToList();
				this.Dispatcher.RequestMainThreadAction(() =>
				{
					lock(_lockObject)
					{
						foreach(var vm in vms)
						{
							_vehicles.Add(vm);
						}
					}

					this.IsBusy = false;

					this.UpdateForecastAsync()
						.ContinueWith(task1 => MvxTrace.Trace("Finished updating forecasts"))
						.ConfigureAwait(false);
				});
			}
			else
			{
				this.Dispatcher.RequestMainThreadAction(() => 
				{
					this.NoData = true;
					this.IsBusy = false;
				});
			}
		}

		private VehicleForecastViewModel CreateFromVehicle(Vehicle vehicle)
		{
			var vm = Mvx.IocConstruct<VehicleForecastViewModel>();
			vm.InitWithData(vehicle, this.Route, false);

			return vm;
		}

		private void OnNextInterval(long interval)
		{
			this.Dispatcher.RequestMainThreadAction(() =>
			{
				lock (_lockObject)
				{
					foreach (var vm in _vehicles) 
					{
						vm.CountdownCommand.Execute();
					}
				}
			});
		}

		private async Task UpdateForecastAsync()
		{
			await Task.Factory.StartNew(() =>
			{
				MvxTrace.Trace("There are {0} vehicles in the list", this.Vehicles.Count());
				foreach (var vehicle in this.Vehicles) 
				{
					MvxTrace.Trace("Updating forecast for {0}", vehicle.Vehicle.CarPlate);
					vehicle.UpdateForecastCommand.Execute();
					MvxTrace.Trace("Forecast for {0} updated", vehicle.Vehicle.CarPlate);
				}
			});
		}
	}
}