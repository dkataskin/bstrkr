using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Cirrious.MvvmCross.ViewModels;

using Xamarin;

using bstrkr.core;
using bstrkr.core.services.location;
using bstrkr.providers;

namespace bstrkr.mvvm.viewmodels
{
	public class UmbrellaRouteViewModel : BusTrackerViewModelBase
	{
		private readonly IBusTrackerLocationService _locationService;
		private readonly ILiveDataProviderFactory _providerFactory;

		private readonly ObservableCollection<RouteViewModel> _routes = new ObservableCollection<RouteViewModel>();

		private string _name;

		public UmbrellaRouteViewModel(IBusTrackerLocationService locationService, ILiveDataProviderFactory providerFactory)
		{
			_locationService = locationService;
			_providerFactory = providerFactory;
			this.Routes = new ReadOnlyObservableCollection<RouteViewModel>(_routes);
		}

		public string Name 
		{ 
			get
			{
				return _name;
			} 

			private set
			{
				if (!string.Equals(_name, value))
				{
					_name = value;
					this.RaisePropertyChanged(() => this.Name);
				}
			} 
   		}

		public ReadOnlyObservableCollection<RouteViewModel> Routes { get; private set; }

		public void Init(string name, IEnumerable<string> routes)
		{
			this.Name = name;

			var liveDataProvider = _providerFactory.CreateProvider(_locationService.Area);
			liveDataProvider.GetRoutesAsync().ContinueWith(task =>
			{
				try 
				{
					this.Dispatcher.RequestMainThreadAction(() =>
					{
						if (task.Result != null)
						{
							foreach (var route in task.Result) 
							{
								if (routes.Contains(route.Id))
								{
									foreach (var routeVM in this.CreateRouteViewModels(route)) 
									{
										_routes.Add(routeVM);
									}
								}
							}
						}
					});
				} 
				catch (Exception e) 
				{
					Insights.Report(e, ReportSeverity.Warning);
				}
				finally
				{
					this.Dispatcher.RequestMainThreadAction(() => this.IsBusy = false);
				}
			});

		}

		private IEnumerable<RouteViewModel> CreateRouteViewModels(Route route)
		{
			var vms = new List<RouteViewModel>();
			foreach (var vehicleType in route.VehicleTypes)
			{
				var vm = new RouteViewModel 
				{
					Id = route.Id,
					Name = route.Name,
					VehicleType = vehicleType
				};

				if (route.FirstStop != null)
				{
					vm.From = route.FirstStop.Name;
				}

				if (route.LastStop != null)
				{
					vm.To = route.LastStop.Name;
				}

				vms.Add(vm);
			}

			return vms;
		}
	}
}