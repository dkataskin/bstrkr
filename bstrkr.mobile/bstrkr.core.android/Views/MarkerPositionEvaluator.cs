using System;

using Android.Animation;
using Android.Gms.Maps.Model;

namespace bstrkr.core.android.views
{
	public class MarkerPositionEvaluator : Java.Lang.Object, ITypeEvaluator
	{
		public Java.Lang.Object Evaluate(float fraction, Java.Lang.Object startValue, Java.Lang.Object endValue)
		{
			var b = endValue as LatLng;
			var a = startValue as LatLng;

			double lat = (b.Latitude - a.Latitude) * fraction + a.Latitude;
			double lngDelta = b.Longitude - a.Longitude;

			// Take the shortest path across the 180th meridian.
			if (Math.Abs(lngDelta) > 180) 
			{
				lngDelta -= Math.Sign(lngDelta) * 360;
			}

			double lng = lngDelta * fraction + a.Longitude;
			return new LatLng(lat, lng);
		}
	}
}