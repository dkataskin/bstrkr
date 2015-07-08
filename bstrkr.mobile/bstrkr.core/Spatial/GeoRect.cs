using System;

namespace bstrkr.core.spatial
{
	public struct GeoRect
	{
		public GeoRect(GeoPoint northEast, GeoPoint southWest)
		{
			this.NorthEast = northEast;
			this.SouthWest = southWest;
		}

		public GeoPoint NorthEast;

		public GeoPoint SouthWest;

		public static GeoRect EarthWide
		{
			get { return new GeoRect(new GeoPoint(0, 0), new GeoPoint(90, 180)); }
		}

		public bool ContainsPoint(GeoPoint point)
		{
			return point.Latitude >= SouthWest.Latitude && point.Latitude <= NorthEast.Latitude &&
				   point.Longitude >= SouthWest.Longitude && point.Longitude <= NorthEast.Longitude;
		}
	}
}