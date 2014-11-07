using System;
using System.Collections.Generic;

using bstrkr.core;

namespace bstrkr.providers.bus13.data
{
	public class Bus13VehicleLocationUpdate
	{
		public Vehicle Vehicle { get; set;}

		public DateTime LastUpdate { get; set; }

		public IList<Waypoint> Waypoints { get; set; }
	}
}