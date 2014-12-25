using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Cirrious.MvvmCross.ViewModels;

using bstrkr.core;

namespace bstrkr.mvvm.viewmodels
{
	public class UmbrellaRouteViewModel : MvxViewModel
	{
		private string _name;

		public UmbrellaRouteViewModel(string name, IEnumerable<RouteViewModel> routeVMs)
		{
			this.Name = name;

			var observableRoutes = new ObservableCollection<RouteViewModel>(routeVMs);
			this.Routes = new ReadOnlyObservableCollection<RouteViewModel>(observableRoutes);
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
	}
}