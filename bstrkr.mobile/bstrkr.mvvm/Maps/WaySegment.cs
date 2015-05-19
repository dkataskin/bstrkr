using System;
using System.Collections.Generic;

using bstrkr.core;
using bstrkr.core.spatial;

namespace bstrkr.mvvm.maps
{
	public class WaySegment
	{
		public TimeSpan Duration { get; set; }

		public GeoPoint StartPosition { get; set; }

		public GeoPoint FinalPosition { get; set; }
	}
}