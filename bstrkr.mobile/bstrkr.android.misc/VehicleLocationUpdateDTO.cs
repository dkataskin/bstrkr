using System;
using System.Collections.Generic;

namespace bstrkr.android.misc
{
	public class VehicleLocationUpdateDTO
	{
		public DateTime ReceivedAt { get; set; }

		public DateTime LastUpdatedAt { get; set; }

		public string VehicleId { get; set; }

		public double Latitude { get; set; }

		public double Longitude { get; set; }

		public double Heading { get; set; }

		public List<WaypointDTO> Waypoints { get; set; }
	}
}