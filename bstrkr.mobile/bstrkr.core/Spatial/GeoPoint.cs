using System;

namespace bstrkr.core.spatial
{
	public struct GeoPoint
	{
		private static GeoPoint EmptyLocation = new GeoPoint(0, 0);

		public GeoPoint(double latitude, double longitude)
		{
			this.Latitude = latitude;
			this.Longitude = longitude;
		}

		public double Latitude;

		public double Longitude;

		public static GeoPoint Empty 
		{ 
			get { return EmptyLocation; }
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