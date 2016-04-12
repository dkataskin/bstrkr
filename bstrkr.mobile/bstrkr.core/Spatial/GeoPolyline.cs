using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace bstrkr.core.spatial
{
    public class GeoPolyline
    {
        public GeoPolyline(IEnumerable<GeoPoint> points)
        {
            if (points == null || !points.Any())
            {
                throw new ArgumentException("Point collection must not be null or empty", nameof(points));
            }

            this.Nodes = new ReadOnlyCollection<GeoPoint>(points.ToList());
        }

        public IReadOnlyCollection<GeoPoint> Nodes { get; private set; }
    }
}