using System;

using bstrkr.core.spatial;

namespace bstrkr.core.utils
{
	public class LinearFixedGeoPointInterpolator : IGeoPointInterpolator
	{
		public GeoPoint Interpolate(float fraction, GeoPoint from, GeoPoint to)
		{
			var lat = (to.Latitude - from.Latitude) * fraction + from.Latitude;
			var lngDelta = to.Longitude - from.Longitude;

			// Take the shortest path across the 180th meridian.
			if (Math.Abs(lngDelta) > 180) 
			{
				lngDelta -= Math.Sign(lngDelta) * 360;
			}

			var lng = lngDelta * fraction + from.Longitude;
			return new GeoPoint(lat, lng);
		}
	}
}