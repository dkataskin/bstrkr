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
		private string _name;

		public UmbrellaRoutesListItemViewModel(string name, IEnumerable<Route> routes)
		{
			this.Name = name;

			var vehicleTypes = routes.SelectMany(x => x.VehicleTypes)
									 .Distinct()
									 .OrderBy(x => x)
									 .ToList();

			var observableVehicleTypes = new ObservableCollection<VehicleTypes>(vehicleTypes);
			this.VehicleTypes = new ReadOnlyObservableCollection<VehicleTypes>(observableVehicleTypes);

			this.Routes = new ReadOnlyCollection<Route>(routes.ToList());
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

		public IReadOnlyCollection<Route> Routes { get; private set; }
	}
}