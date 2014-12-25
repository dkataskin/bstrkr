using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Cirrious.MvvmCross.ViewModels;

using bstrkr.core;

namespace bstrkr.mvvm.viewmodels
{
	public class RouteViewModel : BusTrackerViewModelBase
	{
		private string _id;
		private string _name;
		private string _from;
		private string _to;
		private VehicleTypes _vehicleType;

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
			set
			{
				if (!string.Equals(_name, value))
				{
					_name = value;
					this.RaisePropertyChanged(() => this.Name);
				}
			} 
		}

		public string From
		{
			get { return _from; }
			set
			{
				if (_from != value)
				{
					_from = value;
					this.RaisePropertyChanged(() => this.From);
				}
			}
		}

		public string To
		{
			get { return _to; }
			set
			{
				if (_to != value)
				{
					_to = value;
					this.RaisePropertyChanged(() => this.To);
				}
			}
		}

		public VehicleTypes VehicleType
		{
			get { return _vehicleType; }
			set
			{
				if (_vehicleType != value)
				{
					_vehicleType = value;
					this.RaisePropertyChanged(() => this.VehicleType);
				}
			}
		}
	}
}