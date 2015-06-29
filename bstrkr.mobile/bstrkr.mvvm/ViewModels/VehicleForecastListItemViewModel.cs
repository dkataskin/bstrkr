using System;

using bstrkr.core;

using Cirrious.MvvmCross.ViewModels;

namespace bstrkr.mvvm.viewmodels
{
	public class VehicleForecastListItemViewModel : BusTrackerViewModelBase
	{
		private int _arrivesInSeconds;
		private int _arrivedSeconds;
		private string _routeStopId;
		private string _routeStopName;
		private string _routeStopDescription;
		private VehicleTypes _vehicleType;

		public VehicleForecastListItemViewModel()
		{
			this.CountdownCommand = new MvxCommand(this.Countdown);
		}

		public MvxCommand CountdownCommand { get; private set; }

		public bool IsCurrentRouteStop
		{
			get { return this.ArrivesInSeconds == 0; }
		}

		public int ArrivesInSeconds 
		{ 
			get { return _arrivesInSeconds; } 
			private set
			{
				this.RaiseAndSetIfChanged(ref _arrivesInSeconds, value, () => this.ArrivesInSeconds);
			}
		}

		public int ArrivedSeconds
		{
			get { return _arrivedSeconds; } 
			private set
			{
				this.RaiseAndSetIfChanged(ref _arrivedSeconds, value, () => this.ArrivedSeconds);
			}			
		}

		public VehicleTypes VehicleType
		{
			get { return _vehicleType; }
			set { this.RaiseAndSetIfChanged(ref _vehicleType, value, () => this.VehicleType); }
		}

		public string RouteStopId 
		{ 
			get { return _routeStopId; } 
			private set
			{
				this.RaiseAndSetIfChanged(ref _routeStopId, value, () => this.RouteStopId);
			}
		}

		public string RouteStopName 
		{ 
			get { return _routeStopName; }
			private set
			{
				this.RaiseAndSetIfChanged(ref _routeStopName, value, () => this.RouteStopName);
			}
		}

		public string RouteStopDescription 
		{ 
			get { return _routeStopDescription; } 
			private set
			{
				this.RaiseAndSetIfChanged(ref _routeStopDescription, value, () => this.RouteStopDescription);
			}
   		}

		public void UpdateFromForecastItem(VehicleForecastItem forecastItem)
		{
			this.ArrivesInSeconds = forecastItem.ArrivesInSec;
			if (forecastItem.RouteStop != null)
			{
				this.RouteStopId = forecastItem.RouteStop.Id;
				this.RouteStopName = forecastItem.RouteStop.Name;
				this.RouteStopDescription = forecastItem.RouteStop.Description;
			}
		}

		private void Countdown()
		{
			var routeStopBecameCurrent = false;
			routeStopBecameCurrent = this.ArrivesInSeconds == 0 && this.ArrivedSeconds == 0;

			if (this.ArrivesInSeconds > 0)
			{
				this.ArrivesInSeconds--;
			}
			else
			{
				this.ArrivedSeconds++;
			}

			if (routeStopBecameCurrent)
			{
				this.RaisePropertyChanged(() => this.IsCurrentRouteStop);
			}
		}
	}
}