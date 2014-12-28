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
		private readonly ILiveDataProviderFactory _providerFactory;

		private readonly ObservableCollection<RouteViewModel> _routes = new ObservableCollection<RouteViewModel>();

		private string _title;

		public UmbrellaRouteViewModel(ILiveDataProviderFactory providerFactory)
		{
			_providerFactory = providerFactory;
			this.Routes = new ReadOnlyObservableCollection<RouteViewModel>(_routes);
		}

		public string Title 
		{ 
			get
			{
				return _title;
			} 

			private set
			{
				if (!string.Equals(_title, value))
				{
					_title = value;
					this.RaisePropertyChanged(() => this.Title);
				}
			} 
   		}

		public ReadOnlyObservableCollection<RouteViewModel> Routes { get; private set; }

		public void Init(string name, string routes)
		{
			this.Title = string.Format(AppResources.umbrella_route_title_format, name);

			var routeIds = routes.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
			var liveDataProvider = _providerFactory.GetCurrentProvider();
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
								if (routeIds.Contains(route.Id))
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
					Name = this.GetRouteTitle(route.Number, vehicleType),
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

		private string GetRouteTitle(string number, VehicleTypes vehicleType)
		{
			switch (vehicleType)
			{
				case VehicleTypes.Bus:
					return string.Format(AppResources.bus_route_title_format, number);

				case VehicleTypes.ShuttleBus:
					return string.Format(AppResources.shuttlebus_route_title_format, number);

				case VehicleTypes.Trolleybus:
					return string.Format(AppResources.troll_route_title_format, number);

				case VehicleTypes.Tramway:
					return string.Format(AppResources.tramway_route_title_format, number);

				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}