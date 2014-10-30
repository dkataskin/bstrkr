using System;
using System.Collections.Generic;

using bstrkr.core;

namespace bstrkr.mvvm.maps
{
	public class WaySegment
	{
		public DateTime Timestamp { get; set; }

		public long Duration { get; set; }

		public IList<Waypoint> Waypoints { get; set; }
	}
}