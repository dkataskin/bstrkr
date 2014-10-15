using System;

using bstrkr.core.spatial;

namespace bstrkr.core.consts
{
	public static class AppConsts
	{
		public const string ConfigFileName = "config.json";
		public const double MaxDistance = 50.0;

		public static GeoPoint DefaultLocation = new GeoPoint(55.7503798d, 37.6182293d);
		public const float DefaultZoom = 14.0f;
	}
}