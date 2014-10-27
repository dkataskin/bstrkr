using System;

namespace bstrkr.core
{
	public class VehicleLocationUpdate
	{
		public VehicleLocationUpdate(Vehicle vehicle, WaypointCollection waypoints)
		{
			this.Vehicle = vehicle;
			this.Waypoints = waypoints;
		}

		public Vehicle Vehicle { get; private set; }

		public WaypointCollection Waypoints { get; private set; }
	}
}