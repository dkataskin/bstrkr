using System;

using bstrkr.core;

using Cirrious.MvvmCross.ViewModels;

namespace bstrkr.mvvm.viewmodels
{
	public class RouteStopForecastViewModel : BusTrackerViewModelBase
	{
		private int _arrivesInSeconds;

		public RouteStopForecastViewModel()
		{
			this.CountdownCommand = new MvxCommand(() => this.ArrivesInSeconds--, () => this.ArrivesInSeconds > 0);
		}

		public MvxCommand CountdownCommand { get; private set; }

		public string VehicleId { get; set; }

		public int ArrivesInSeconds 
		{ 
			get { return _arrivesInSeconds; } 
			set { this.RaiseAndSetIfChanged(ref _arrivesInSeconds, value, () => this.ArrivesInSeconds); } 
		}

		public string CurrentlyAt { get; set; }

		public string LastStop { get; set; }

		public string RouteDisplayName { get; set; }

		public Route Route { get; set; }

		public Route ParentRoute { get; set; }
	}
}