using System;

using MonoTouch.CoreLocation;

using bstrkr.core.spatial;

namespace bstrkr.core.ios.extensions
{
	public static class CLLocationCoordinate2DExtensions
	{
		public static GeoPoint ToGeoPoint(this CLLocationCoordinate2D location)
		{
			return new GeoPoint(location.Latitude, location.Longitude);
		}
	}
}