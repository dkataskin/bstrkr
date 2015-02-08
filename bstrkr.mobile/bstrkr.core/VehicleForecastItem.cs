using System;

namespace bstrkr.core
{
	public class VehicleForecastItem
	{
		public VehicleForecastItem(RouteStop routeStop, int arrivesInSec)
		{
			this.ArrivesInSec = arrivesInSec;
			this.RouteStop = routeStop;
		}

		public int ArrivesInSec { get; private set; }

		public RouteStop RouteStop { get; private set; }
	}
}