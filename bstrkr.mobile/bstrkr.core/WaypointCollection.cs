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

		public WaypointCollection() : this(new List<Waypoint>())
		{
		}

		public WaypointCollection(IEnumerable<Waypoint> waypoints)
		{
			if (waypoints == null || !waypoints.Any())
			{
				_waypoints = new List<Waypoint>();
			}
			else
			{
				_waypoints = waypoints.ToList();
			}

			this.Waypoints = new ReadOnlyCollection<Waypoint>(_waypoints);
		}

		public TimeSpan Duration { get; set; }

		public IReadOnlyList<Waypoint> Waypoints { get; private set; }

		public void Add(GeoPoint position, float heading, float fraction)
		{
			_waypoints.Add(new Waypoint(position, heading, fraction));
		}
	}
}