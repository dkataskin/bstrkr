using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using bstrkr.core;
using bstrkr.core.services.location;
using bstrkr.mvvm.converters;
using bstrkr.providers;

using Cirrious.MvvmCross.ViewModels;

using Xamarin;

namespace bstrkr.mvvm.viewmodels
{
	public class UmbrellaRouteViewModel : BusTrackerViewModelBase
	{
		private readonly ILiveDataProviderFactory _providerFactory;
		private readonly RouteNumberToTitleConverter _routeNumberConverter = new RouteNumberToTitleConverter();
		private readonly ObservableCollection<RoutesListItemViewModel> _routes = new ObservableCollection<RoutesListItemViewModel>();

		private string _title;

		public UmbrellaRouteViewModel(ILiveDataProviderFactory providerFactory)
		{
			_providerFactory = providerFactory;
			this.Routes = new ReadOnlyObservableCollection<RoutesListItemViewModel>(_routes);

			this.ShowRouteVehiclesCommand = new MvxCommand<RoutesListItemViewModel>(this.ShowRouteVehicles);
		}

		public MvxCommand<RoutesListItemViewModel> ShowRouteVehiclesCommand { get; private set; }

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

		public ReadOnlyObservableCollection<RoutesListItemViewModel> Routes { get; private set; }

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

		private IEnumerable<RoutesListItemViewModel> CreateRouteViewModels(Route route)
		{
			var vms = new List<RoutesListItemViewModel>();
			foreach (var vehicleType in route.VehicleTypes)
			{
				var vm = new RoutesListItemViewModel 
				{
					Id = route.Id,
					Name = this.GetRouteTitle(route.Number, vehicleType),
					VehicleType = vehicleType,
					Route = route
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
			return _routeNumberConverter.Convert(number, vehicleType);
		}

		private void ShowRouteVehicles(RoutesListItemViewModel selectedRoute)
		{
			var ids = string.Empty;
			if (selectedRoute.Route != null && selectedRoute.Route.Ids != null)
			{
				ids = string.Join(",", selectedRoute.Route.Ids);
			}

			this.ShowViewModel<RouteViewModel>(new 
			{ 
				routeId = selectedRoute.Id, 
				routeName = selectedRoute.Name,
				routeNumber = selectedRoute.Route.Number,
				routeIds = ids,
				vehicleType = selectedRoute.VehicleType
			});
		}
	}
}