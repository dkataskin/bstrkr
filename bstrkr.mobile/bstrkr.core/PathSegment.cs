using System;

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