using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Cirrious.MvvmCross.ViewModels;

using bstrkr.core;

namespace bstrkr.mvvm.viewmodels
{
	public class RoutesListItemViewModel : BusTrackerViewModelBase
	{
		private string _name;
		private VehicleTypes _vehicleType;
		private ReadOnlyObservableCollection<Route> _routes;

		public RoutesListItemViewModel(IEnumerable<Route> routes, string name, VehicleTypes vehicleType)
		{
			this.Routes = new ReadOnlyObservableCollection<Route>(new ObservableCollection<Route>(routes));
			this.Name = name;
			this.VehicleType = vehicleType;
		}

		public string Name 
		{ 
			get { return _name; } 
			private set { this.RaiseAndSetIfChanged(ref _name, value, () => this.Name); } 
		}

		public VehicleTypes VehicleType
		{
			get { return _vehicleType; }
			private set { this.RaiseAndSetIfChanged(ref _vehicleType, value, () => this.VehicleType); }
		}

		public ReadOnlyObservableCollection<Route> Routes 
		{ 
			get { return _routes; }
			private set { this.RaiseAndSetIfChanged(ref _routes, value, () => this.Routes); } 
		}
	}
}