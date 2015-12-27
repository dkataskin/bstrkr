using System;

namespace bstrkr.core
{
	public class RouteStopForecastItem
	{
		public int ArrivesInSeconds { get; set; }

		public string VehicleId { get; set; }

		public string CurrentRouteStopName { get; set; }

		public string LastRouteStopName { get; set; }

		public Route Route { get; set; }
	}
}