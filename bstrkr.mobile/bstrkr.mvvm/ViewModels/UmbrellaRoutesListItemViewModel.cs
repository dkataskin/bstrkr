using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using bstrkr.core;

using Cirrious.MvvmCross.ViewModels;

namespace bstrkr.mvvm.viewmodels
{
	public class UmbrellaRoutesListItemViewModel : BusTrackerViewModelBase
	{
		private string _routeNumber;
		private ObservableCollection<RoutesListItemViewModel> _routes = new ObservableCollection<RoutesListItemViewModel>();

		public UmbrellaRoutesListItemViewModel(string name, IEnumerable<Route> routes)
		{
			this.RouteNumber = name;

			this.Routes = new ReadOnlyObservableCollection<RoutesListItemViewModel>(_routes);

			foreach (var route in routes)
			{
				foreach(var vm in UmbrellaRouteViewModel.CreateRouteViewModels(route))
				{
					_routes.Add(vm);
				}
			}
		}

		public string RouteNumber 
		{ 
			get
			{
				return _routeNumber;
			} 

			private set
			{
				if (!string.Equals(_routeNumber, value))
				{
					_routeNumber = value;
					this.RaisePropertyChanged(() => this.RouteNumber);
				}
			} 
		}

		public ReadOnlyObservableCollection<RoutesListItemViewModel> Routes { get; private set; }
	}
}