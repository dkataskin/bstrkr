using System;
using System.Collections.ObjectModel;
using System.Linq;

using Xamarin;

using bstrkr.core.services.location;
using bstrkr.providers;
using bstrkr.core;
using System.Collections.Generic;

namespace bstrkr.mvvm.viewmodels
{
	public class RoutesViewModel : BusTrackerViewModelBase
	{
		private readonly ObservableCollection<UmbrellaRouteViewModel> _routes = new ObservableCollection<UmbrellaRouteViewModel>();

		private bool _unknownArea;

		public RoutesViewModel(IBusTrackerLocationService locationService, ILiveDataProviderFactory providerFactory)
		{
			this.Routes = new ReadOnlyObservableCollection<UmbrellaRouteViewModel>(_routes);
			this.UnknownArea = locationService.Area == null;

			if (locationService.Area != null)
			{
				this.IsBusy = true;

				var provider = providerFactory.CreateProvider(locationService.Area);

				provider.GetRoutesAsync().ContinueWith(task =>
				{
					try 
					{
						this.Dispatcher.RequestMainThreadAction(() =>
						{
							if (task.Result != null)
							{
								foreach (var routeGroup in task.Result.GroupBy(x => x.Number)) 
								{
									_routes.Add(new UmbrellaRouteViewModel(
													routeGroup.Key,
													routeGroup.SelectMany(x => this.CreateRouteViewModels(routeGroup.Key, x))
															  .ToList()));
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
		}

		public bool UnknownArea
		{
			get 
			{
				return _unknownArea;
			}

			set
			{
				if (_unknownArea != value)
				{
					_unknownArea = value;
					this.RaisePropertyChanged(() => this.UnknownArea);
				}
			}
		}

		public ReadOnlyObservableCollection<UmbrellaRouteViewModel> Routes { get; private set; }

		private IEnumerable<RouteViewModel> CreateRouteViewModels(string name, Route route)
		{
			var vms = new List<RouteViewModel>();
			foreach (var vehicleType in route.VehicleTypes)
			{
				var vm = new RouteViewModel 
				{
					Id = route.Id,
					Name = name,
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