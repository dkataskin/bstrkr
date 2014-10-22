using System;

using bstrkr.core.spatial;

namespace bstrkr.core.utils
{
	public class LinearGeoPointInterpolator : IGeoPointInterpolator
	{
		public GeoPoint Interpolate(float fraction, GeoPoint a, GeoPoint b)
		{
			var lat = (b.Latitude - a.Latitude) * fraction + a.Latitude;
			var lng = (b.Longitude - a.Longitude) * fraction + a.Longitude;
			return new GeoPoint(lat, lng);
		}
	}
}