using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using bstrkr.core;
using bstrkr.core.spatial;
using bstrkr.mvvm.viewmodels;
using bstrkr.providers;

using Cirrious.MvvmCross.ViewModels;

using Xamarin;

namespace bstrkr.mvvm.viewmodels
{
	public class RouteViewModel : BusTrackerViewModelBase
	{
		private readonly ILiveDataProviderFactory _providerFactory;
		private readonly ObservableCollection<RouteVehiclesListItemViewModel> _vehicles = new ObservableCollection<RouteVehiclesListItemViewModel>();

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
			if (vehicles != null)
			{
				var vms = vehicles.Select(this.CreateFromVehicle).ToList();
				this.Dispatcher.RequestMainThreadAction(() =>
				{
					foreach(var vm in vms)
					{
						_vehicles.Add(vm);
					}
				});
			}

			this.Dispatcher.RequestMainThreadAction(() => this.IsBusy = false);
			this.UpdateForecast();
		}

		private RouteVehiclesListItemViewModel CreateFromVehicle(Vehicle vehicle)
		{
			return new RouteVehiclesListItemViewModel(vehicle);
		}

		private void UpdateForecast()
		{
			Task.Factory.StartNew(() =>
			{
				foreach (var vehicle in this.Vehicles) 
				{
					vehicle.Update();
				}
			});
		}
	}
}