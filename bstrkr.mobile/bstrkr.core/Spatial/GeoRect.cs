using System;

namespace bstrkr.core.spatial
{
	public struct GeoRect
	{
		public GeoRect(GeoPoint leftTop, GeoPoint rightBottom)
		{
			this.LeftTop = leftTop;
			this.RightBottom = rightBottom;
		}

		public GeoPoint LeftTop;

		public GeoPoint RightBottom;

		public static GeoRect EarthWide
		{
			get { return new GeoRect(new GeoPoint(0, 0), new GeoPoint(90, 180)); }
		}
	}
}