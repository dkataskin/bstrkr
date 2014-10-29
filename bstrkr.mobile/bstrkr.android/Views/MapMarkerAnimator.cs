using System;

using Android.Animation;
using Android.Gms.Maps.Model;

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
			var googleMapMarker = (marker as GoogleMapsMarkerBase).Marker;
			var evaluator = new GeoPointEvaluator(interpolator);
			var animator = ObjectAnimator.OfObject(googleMapMarker, "Position", evaluator, finalPosition.ToLatLng())
						  				 .SetDuration(duration);

			animator.AnimationEnd += (sender, args) => this.RaiseAnimationEndEvent();
		}

		public event EventHandler<EventArgs> AnimationEnd;			

		private void RaiseAnimationEndEvent()
		{
			if (this.AnimationEnd != null)
			{
				this.AnimationEnd(this, EventArgs.Empty);
			}
		}

		private class GeoPointEvaluator : Java.Lang.Object, ITypeEvaluator
		{
			private readonly IGeoPointInterpolator _interpolator;

			public GeoPointEvaluator(IGeoPointInterpolator interpolator)
			{
				_interpolator = interpolator;
			}

			public Java.Lang.Object Evaluate(float fraction, Java.Lang.Object startValue, Java.Lang.Object endValue)
			{
				var latLngStart = startValue as LatLng;
				var latLngStop = endValue as LatLng;

				return _interpolator.Interpolate(fraction, latLngStart.ToGeoPoint(), latLngStop.ToGeoPoint()).ToLatLng();
			}
		}
	}
}