using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
	public class RouteViewModel : BusTrackerViewModelBase
	{
		private readonly ILiveDataProviderFactory _providerFactory;
		private readonly ObservableCollection<RouteVehiclesListItemViewModel> _vehicles = new ObservableCollection<RouteVehiclesListItemViewModel>();

		private bool _noData;
		private string _routeId;
		private string _name;
		private string _from;
		private string _to;
		private int _number;
		private VehicleTypes _vehicleType;

		public RouteViewModel(ILiveDataProviderFactory providerFactory)
		{
			_providerFactory = providerFactory;
			this.Vehicles = new ReadOnlyObservableCollection<RouteVehiclesListItemViewModel>(_vehicles);
			this.ShowRouteVehicleDetailsCommand = new MvxCommand(() => {});
		}

		public MvxCommand ShowRouteVehicleDetailsCommand { get; private set; }

		public string RouteId
		{
			get { return _routeId; }
			private set
			{
				if (_routeId != value)
				{
					_routeId = value;
					this.RaisePropertyChanged(() => this.RouteId);
				}
			}
		}

		public int Number
		{
			get { return _number; }
			private set 
			{
				if (_number != value)
				{
					_number = value;
					this.RaisePropertyChanged(() => this.Number);
				}
			}
		}

		public string Name 
		{ 
			get { return _name; } 
			private set
			{
				if (!string.Equals(_name, value))
				{
					_name = value;
					this.RaisePropertyChanged(() => this.Name);
				}
			} 
		}

		public string From
		{
			get { return _from; }
			set
			{
				if (_from != value)
				{
					_from = value;
					this.RaisePropertyChanged(() => this.From);
				}
			}
		}

		public string To
		{
			get { return _to; }
			set
			{
				if (_to != value)
				{
					_to = value;
					this.RaisePropertyChanged(() => this.To);
				}
			}
		}

		public bool NoData
		{
			get { return _noData; }
			private set
			{
				this.RaiseAndSetIfChanged(ref _noData, value, () => this.NoData);
			}
		}

		public VehicleTypes VehicleType
		{
			get { return _vehicleType; }
			private set
			{
				if (_vehicleType != value)
				{
					_vehicleType = value;
					this.RaisePropertyChanged(() => this.VehicleType);
				}
			}
		}

		public Route Route { get; set; }

		public ReadOnlyObservableCollection<RouteVehiclesListItemViewModel> Vehicles { get; private set; }

		public ReadOnlyObservableCollection<RouteStop> Stops { get; private set; }

		public void Init(string routeId, string routeName, int routeNumber, string routeIds, VehicleTypes vehicleType)
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
				provider.GetRouteVehiclesAsync(new[] { this.Route })
						.ContinueWith(this.ShowRouteVehicles)
						.ConfigureAwait(false);
			}
		}

		private void ShowRouteVehicles(Task<IEnumerable<Vehicle>> task)
		{
			if (task.Status != TaskStatus.RanToCompletion)
			{
				Insights.Report(task.Exception, ReportSeverity.Error);
				return;
			}

			var vehicles = task.Result;
			if (vehicles != null && vehicles.Any())
			{
				var vms = vehicles.Select(this.CreateFromVehicle).ToList();
				this.Dispatcher.RequestMainThreadAction(() =>
				{
					foreach(var vm in vms)
					{
						_vehicles.Add(vm);
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

		private RouteVehiclesListItemViewModel CreateFromVehicle(Vehicle vehicle)
		{
			var vm = Mvx.IocConstruct<RouteVehiclesListItemViewModel>();
			vm.Vehicle = vehicle;

			return vm;
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