using System;

using MonoTouch.CoreLocation;

using bstrkr.core.spatial;

namespace bstrkr.core.ios.extensions
{
	public static class GeoPointExtensions
	{
		public static CLLocationCoordinate2D ToCLLocation(this GeoPoint point)
		{
			return new CLLocationCoordinate2D(point.Latitude, point.Longitude);
		}
	}
}