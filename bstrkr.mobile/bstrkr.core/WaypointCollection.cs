using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using bstrkr.core.spatial;

namespace bstrkr.core
{
	public class WaypointCollection
	{
		private readonly IList<Waypoint> _waypoints = new List<Waypoint>();

		public WaypointCollection()
		{
			this.Waypoints = new ReadOnlyCollection<Waypoint>(_waypoints);
		}

		public TimeSpan TimeSpan { get; set; }

		public IReadOnlyList<Waypoint> Waypoints { get; private set; }

		public void Add(GeoPoint location, float heading, float fraction)
		{
			_waypoints.Add(new Waypoint(location, heading, fraction));
		}
	}
}