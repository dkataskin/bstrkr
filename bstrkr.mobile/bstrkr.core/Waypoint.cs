using System;

using bstrkr.core.spatial;

namespace bstrkr.core
{
	public struct Waypoint
	{
		public Waypoint(GeoPoint location, float heading, float fraction)
		{
			Location = location;
			Fraction = fraction;
			Heading = heading;
		}

		public float Heading;

		public GeoPoint Location;

		public float Fraction;
	}
}