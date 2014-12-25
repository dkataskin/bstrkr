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

			var vehicleTypes = routeVMs.Select(x => x.VehicleType).OrderBy(x => x);
			var observableVehicleTypes = new ObservableCollection<VehicleTypes>(vehicleTypes);
			this.VehicleTypes = new ReadOnlyObservableCollection<VehicleTypes>(observableVehicleTypes);
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

		public ReadOnlyObservableCollection<VehicleTypes> VehicleTypes { get; private set; }
	}
}