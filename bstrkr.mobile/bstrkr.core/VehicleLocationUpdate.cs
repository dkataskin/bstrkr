using System;

namespace bstrkr.core
{
	public class VehicleLocationUpdate
	{
		public VehicleLocationUpdate(Vehicle vehicle, WaypointCollection waypoints, DateTime lastUpdated)
		{
			this.Vehicle = vehicle;
			this.Waypoints = waypoints;
			this.LastUpdated = lastUpdated;
		}

		public Vehicle Vehicle { get; private set; }

		public WaypointCollection Waypoints { get; private set; }

		public DateTime LastUpdated { get; private set; }
	}
}