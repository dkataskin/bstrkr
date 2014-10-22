using System;

using bstrkr.core.spatial;

namespace bstrkr.core.utils
{
	public class LinearGeoPointInterpolator : IGeoPointInterpolator
	{
		public GeoPoint Interpolate(float fraction, GeoPoint from, GeoPoint to)
		{
			var lat = (to.Latitude - from.Latitude) * fraction + from.Latitude;
			var lng = (to.Longitude - from.Longitude) * fraction + from.Longitude;

			return new GeoPoint(lat, lng);
		}
	}
}