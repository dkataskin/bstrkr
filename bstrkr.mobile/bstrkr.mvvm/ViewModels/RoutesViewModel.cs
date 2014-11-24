using System;
using System.Collections.ObjectModel;

using bstrkr.core.services.location;

namespace bstrkr.mvvm.viewmodels
{
	public class RoutesViewModel : BusTrackerViewModelBase
	{
		private readonly ObservableCollection<RouteViewModel> _routes = new ObservableCollection<RouteViewModel>();

		public RoutesViewModel(IBusTrackerLocationService locationService)
		{
			this.Routes = new ReadOnlyObservableCollection<RouteViewModel>(_routes);
		}

		public ReadOnlyObservableCollection<RouteViewModel> Routes { get; private set; }
	}
}