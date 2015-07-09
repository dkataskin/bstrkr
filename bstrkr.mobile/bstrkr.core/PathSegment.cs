using System;
using System.Collections.Generic;

using bstrkr.core;
using bstrkr.core.spatial;

namespace bstrkr.core
{
	public class PathSegment
	{
		public TimeSpan Duration { get; set; }

		public GeoLocation StartLocation { get; set; }

		public GeoLocation FinalLocation { get; set; }
	}
}