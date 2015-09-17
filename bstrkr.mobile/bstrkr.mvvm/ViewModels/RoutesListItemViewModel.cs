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
		private string _id;
		private string _name;
		private VehicleTypes _vehicleType;
		private Route _route;

		public string Id
		{
			get { return _id; }
			set
			{
				if (_id != value)
				{
					_id = value;
					this.RaisePropertyChanged(() => this.Id);
				}
			}
		}

		public string Name 
		{ 
			get { return _name; } 
			set { this.RaiseAndSetIfChanged(ref _name, value, () => this.Name); } 
		}

		public VehicleTypes VehicleType
		{
			get { return _vehicleType; }
			set { this.RaiseAndSetIfChanged(ref _vehicleType, value, () => this.VehicleType); }
		}

		public Route Route 
		{ 
			get { return _route; }
			set { this.RaiseAndSetIfChanged(ref _route, value, () => this.Route); } 
		}
	}
}