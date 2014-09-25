using System;

namespace bstrkr.core.spatial
{
	public struct GeoPoint
	{
		private const double R = 6371.0d;

		private static GeoPoint EmptyLocation = new GeoPoint(0, 0);

		private double _latitude;
		private double _longitude;

		public GeoPoint(double latitude, double longitude)
		{
			this.Latitude = latitude;
			this.Longitude = longitude;
		}

		public double Latitude 
		{
			get 
			{
				return _latitude;
			}

			set 
			{
				if (value < -90.0 || value > 90.0)
				{
					throw new ArgumentOutOfRangeException("value", "Latitude must be between -90 and 90 degrees.");
				}

				_latitude = value;
			}
		}

		public double Longitude
		{
			get
			{
				return _longitude;
			}

			set
			{
				if (value < -180.0 || value > 180.0)
				{
					throw new ArgumentOutOfRangeException("value", "Longitude must be between -180 and 180 degrees.");
				}
			}
		}

		public static GeoPoint Empty 
		{ 
			get { return EmptyLocation; }
		}

		public static double ToRadians(double degrees)
		{
			return (degrees * Math.PI) / 180;
		}

		public double Distance(GeoPoint point)
		{
			var lat1 = ToRadians(this.Latitude);
			var lat2 = ToRadians(point.Latitude);
			var latDelta = ToRadians(point.Latitude - this.Latitude);
			var lonDelta = ToRadians(point.Longitude - this.Longitude);

			var a = Math.Sin(latDelta / 2.0) * Math.Sin(latDelta / 2.0) +
			        Math.Cos(lat1) * Math.Cos(lat2) * Math.Sin(lonDelta / 2.0) * Math.Sin(lonDelta / 2.0);
			var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

			return R * c;
		}

		public override string ToString()
		{
			return string.Format("({0}, {1})", this.Latitude, this.Longitude);
		}

		public override int GetHashCode()
		{
			return this.Latitude.GetHashCode() ^ this.Longitude.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}

			var geoPoint = (GeoPoint)obj;
			return this.Latitude.Equals(geoPoint.Latitude) &&
				   this.Longitude.Equals(geoPoint.Longitude);
		}

	}
}