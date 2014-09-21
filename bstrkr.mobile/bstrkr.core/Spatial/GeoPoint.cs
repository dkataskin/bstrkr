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
	}
}