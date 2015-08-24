using System;
using System.Collections.Generic;

using bstrkr.core;

namespace bstrkr.grabber
{
	public class VehicleLocationUpdateDTO
	{
		public DateTime ReceivedAt { get; set; }

		public DateTime LastUpdatedAt { get; set; }

		public string VehicleId { get; set; }

		public List<WaypointDTO> Waypoints { get; set; }
	}
}

