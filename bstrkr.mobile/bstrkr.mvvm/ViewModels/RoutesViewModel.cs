using System;
using System.Collections.ObjectModel;

using bstrkr.core.services.location;
using bstrkr.providers;

namespace bstrkr.mvvm.viewmodels
{
	public class RoutesViewModel : BusTrackerViewModelBase
	{
		private readonly ObservableCollection<RouteViewModel> _routes = new ObservableCollection<RouteViewModel>();

		public RoutesViewModel(IBusTrackerLocationService locationService, ILiveDataProviderFactory providerFactory)
		{
			this.Routes = new ReadOnlyObservableCollection<RouteViewModel>(_routes);
			if (locationService.Area != null)
			{
				var provider = providerFactory.CreateProvider(locationService.Area);

				provider.GetRoutesAsync().ContinueWith(task =>
				{
					try 
					{
						this.Dispatcher.RequestMainThreadAction(() =>
						{
							foreach (var route in task.Result) 
							{
								_routes.Add(new RouteViewModel 
								{ 
									Name = route.Name,
									From = route.FirstStop.Name,
									To = route.LastStop.Name
								});
							}
						});
					} 
					catch (Exception e) 
					{
					}
					finally
					{
						this.Dispatcher.RequestMainThreadAction(() => this.IsBusy = false);
					}
				});
			}
		}

		public ReadOnlyObservableCollection<RouteViewModel> Routes { get; private set; }
	}
}