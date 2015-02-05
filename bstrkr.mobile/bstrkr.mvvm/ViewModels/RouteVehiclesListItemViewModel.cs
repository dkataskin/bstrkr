using System;

using bstrkr.mvvm.viewmodels;
using bstrkr.core;

namespace bstrkr.mvvm.viewmodels
{
	public class RouteVehiclesListItemViewModel : BusTrackerViewModelBase
	{
		private string _vehicleCarPlateNumber;
		private int _arrivesInSeconds;
		private string _routeStopId;
		private string _routeStopName;
		private string _routeStopDescription;

		public RouteVehiclesListItemViewModel(Vehicle vehicle)
		{
			this.VehicleId = vehicle.Id;
			this.VehicleCarPlateNumber = vehicle.CarPlate;
		}

		public string VehicleId { get; set; }

		public string VehicleCarPlateNumber
		{
			get { return _vehicleCarPlateNumber; }
			private set
			{
				if (_vehicleCarPlateNumber != value)
				{
					_vehicleCarPlateNumber = value;
					this.RaisePropertyChanged(() => this.VehicleCarPlateNumber);
				}
			}
		}

		public int ArrivesInSeconds 
		{ 
			get { return _arrivesInSeconds; } 
			set
			{
				if (_arrivesInSeconds != value)
				{
					_arrivesInSeconds = value;
					this.RaisePropertyChanged(() => this.ArrivesInSeconds);
				}
			}
		}

		public string RouteStopId 
		{ 
			get { return _routeStopId; } 
			set
			{
				if (_routeStopId != value)
				{
					_routeStopId = value;
					this.RaisePropertyChanged(() => this.RouteStopId);
				}
			}
		}

		public string RouteStopName 
		{ 
			get { return _routeStopName; }
			set
			{
				if (_routeStopName != value)
				{
					_routeStopName = value;
					this.RaisePropertyChanged(() => this.RouteStopName);
				}
			}
		}

		public string RouteStopDescription 
		{ 
			get { return _routeStopDescription; } 
			set
			{
				if (_routeStopDescription != value)
				{
					_routeStopDescription = value;
					this.RaisePropertyChanged(() => this.RouteStopDescription);
				}
			}
		}
	}
}