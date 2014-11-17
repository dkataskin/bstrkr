using System;
using System.Collections.ObjectModel;

namespace bstrkr.mvvm.viewmodels
{
	public class RoutesViewModel : BusTrackerViewModelBase
	{
		private readonly ObservableCollection<RouteViewModel> _routes = new ObservableCollection<RouteViewModel>();

		public RoutesViewModel()
		{
			this.Routes = new ReadOnlyObservableCollection<RouteViewModel>(_routes);
		}

		public ReadOnlyObservableCollection<RouteViewModel> Routes { get; private set; }
	}
}