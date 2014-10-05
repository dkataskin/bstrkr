using System;

using Cirrious.MvvmCross.ViewModels;

using bstrkr.core;
using bstrkr.core.spatial;

namespace bstrkr.mvvm.viewmodels
{
	public class VehicleViewModel : MvxViewModel
	{
		private GeoPoint _location;
		private string _vehicleId;
		private string _carPlate;

		public VehicleViewModel(Vehicle vehicle)
		{
			VehicleId = vehicle.Id;
			Location = vehicle.Location;
			CarPlate = vehicle.CarPlate;
		}

		public string VehicleId
		{
			get 
			{ 
				return _vehicleId; 
			}

			private set 
			{
				if (!string.Equals(_vehicleId, value))
				{
					_vehicleId = value;
				}
			}
		}

		public string CarPlate
		{
			get 
			{ 
				return _carPlate; 
			}

			private set 
			{ 
				if (!string.Equals(_carPlate, value))
				{
					_carPlate = value;
				}
			}
		}

		public GeoPoint Location
		{
			get 
			{ 
				return _location; 
			}

			set 
			{
				if (!_location.Equals(value))
				{
					_location = value;
					this.RaisePropertyChanged(() => this.Location);
				}
			}
		}
	}
}