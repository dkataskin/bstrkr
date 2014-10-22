using System;

using bstrkr.core.spatial;

namespace bstrkr.core.utils
{
	public class LinearFixedGeoPointInterpolator : IGeoPointInterpolator
	{
		public GeoPoint Interpolate(float fraction, GeoPoint a, GeoPoint b)
		{
			var lat = (b.Latitude - a.Latitude) * fraction + a.Latitude;
			var lngDelta = b.Longitude - a.Longitude;

			// Take the shortest path across the 180th meridian.
			if (Math.Abs(lngDelta) > 180) {
				lngDelta -= Math.Sign(lngDelta) * 360;
			}

			var lng = lngDelta * fraction + a.Longitude;
			return new GeoPoint(lat, lng);
		}
	}
}