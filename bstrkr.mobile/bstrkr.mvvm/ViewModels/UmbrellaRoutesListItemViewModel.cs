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

		public UmbrellaRoutesListItemViewModel(string name, IEnumerable<Route> routes)
		{
			this.RouteNumber = name;

			var vehicleTypes = routes.SelectMany(x => x.VehicleTypes)
									 .Distinct()
									 .OrderBy(x => x)
									 .ToList();

			var observableVehicleTypes = new ObservableCollection<VehicleTypes>(vehicleTypes);
			this.VehicleTypes = new ReadOnlyObservableCollection<VehicleTypes>(observableVehicleTypes);

			this.Routes = new ReadOnlyCollection<Route>(routes.ToList());
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

		public ReadOnlyObservableCollection<VehicleTypes> VehicleTypes { get; private set; }

		public IReadOnlyCollection<Route> Routes { get; private set; }
	}
}