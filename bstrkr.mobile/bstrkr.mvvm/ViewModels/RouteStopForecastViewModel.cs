using System;

using bstrkr.core;

namespace bstrkr.mvvm.viewmodels
{
	public class RouteStopForecastViewModel : BusTrackerViewModelBase
	{
		public string VehicleId { get; set; }

		public int ArrivesInSeconds { get; set; }

		public string CurrentlyAt { get; set; }

		public string LastStop { get; set; }

		public string RouteDisplayName { get; set; }

		public Route Route { get; set; }

		public Route ParentRoute { get; set; }
	}
}