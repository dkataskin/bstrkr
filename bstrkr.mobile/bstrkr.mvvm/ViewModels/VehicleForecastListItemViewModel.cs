using System;

using Cirrious.MvvmCross.ViewModels;
using bstrkr.core;

namespace bstrkr.mvvm.viewmodels
{
	public class VehicleForecastListItemViewModel : BusTrackerViewModelBase
	{
		private int _arrivesInSeconds;
		private string _routeStopId;
		private string _routeStopName;
		private string _routeStopDescription;

		public VehicleForecastListItemViewModel()
		{
			this.CountdownCommand = new MvxCommand(this.Countdown, () => this.ArrivesInSeconds > 0);
		}

		public MvxCommand CountdownCommand { get; private set; }

		public int ArrivesInSeconds 
		{ 
			get { return _arrivesInSeconds; } 
			private set
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
			private set
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
			private set
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
			private set
			{
				if (_routeStopDescription != value)
				{
					_routeStopDescription = value;
					this.RaisePropertyChanged(() => this.RouteStopDescription);
				}
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
			this.ArrivesInSeconds--;
		}
	}
}