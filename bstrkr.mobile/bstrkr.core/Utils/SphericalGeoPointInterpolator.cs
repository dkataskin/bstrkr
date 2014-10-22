using System;

using bstrkr.core.spatial;
using bstrkr.core.extensions;

namespace bstrkr.core.utils
{
	public class SphericalGeoPointInterpolator : IGeoPointInterpolator
	{
		public GeoPoint Interpolate(float fraction, GeoPoint from, GeoPoint to)
		{
			// http://en.wikipedia.org/wiki/Slerp
			double fromLat = from.Latitude.ToRadians();
			double fromLng = from.Longitude.ToRadians();
			double toLat = to.Latitude.ToRadians();
			double toLng = to.Longitude.ToRadians();
			double cosFromLat = Math.Cos(fromLat);
			double cosToLat = Math.Cos(toLat);

			// Computes Spherical interpolation coefficients.
			var angle = this.ComputeAngleBetween(fromLat, fromLng, toLat, toLng);
			var sinAngle = Math.Sin(angle);
			if (sinAngle < 1E-6) 
			{
				return from;
			}

			var a = Math.Sin((1 - fraction) * angle) / sinAngle;
			var b = Math.Sin(fraction * angle) / sinAngle;

			// Converts from polar to vector and interpolate.
			var x = a * cosFromLat * Math.Cos(fromLng) + b * cosToLat * Math.Cos(toLng);
			var y = a * cosFromLat * Math.Sin(fromLng) + b * cosToLat * Math.Sin(toLng);
			var z = a * Math.Sin(fromLat) + b * Math.Sin(toLat);

			// Converts interpolated vector back to polar.
			var lat = Math.Atan2(z, Math.Sqrt(x * x + y * y));
			var lng = Math.Atan2(y, x);
			return new GeoPoint(lat.ToDegrees(), lng.ToDegrees());
		}

		private double ComputeAngleBetween(double fromLat, double fromLng, double toLat, double toLng) 
		{
			// Haversine's formula
			var dLat = fromLat - toLat;
			var dLng = fromLng - toLng;
			return 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(dLat / 2), 2) +
				Math.Cos(fromLat) * Math.Cos(toLat) * Math.Pow(Math.Sin(dLng / 2), 2)));
		}
	}
}