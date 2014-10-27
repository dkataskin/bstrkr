using System;

using Android.Animation;

using bstrkr.core.android.extensions;
using bstrkr.core.spatial;
using bstrkr.core.utils;
using bstrkr.mvvm.maps;
using bstrkr.mvvm.views;

namespace bstrkr.android.views
{
	public class MapMarkerAnimator : IMapMarkerAnimator
	{
		public void Animate(IMapMarker marker, GeoPoint finalPosition, IGeoPointInterpolator interpolator, long duration)
		{
			//var typeEvaluator = new TypeEvaluator<LatLng>() {
//				public LatLng evaluate(float fraction, LatLng startValue, LatLng endValue) {
//					return latLngInterpolator.interpolate(fraction, startValue, endValue);
//				}
//			};

			var googleMapMarker = (marker as GoogleMapsMarkerBase).Marker;

			ObjectAnimator.OfObject(googleMapMarker, "Position", null, finalPosition.ToLatLng())
						  .SetDuration(duration)
						  .Start();
		}



//		private class GeoPointEvaluator : ITypeEvaluator
//		{
//			public Java.Lang.Object Evaluate(float fraction, Java.Lang.Object startValue, Java.Lang.Object endValue)
//			{
//				return null;
//			}
//		}
	}
}