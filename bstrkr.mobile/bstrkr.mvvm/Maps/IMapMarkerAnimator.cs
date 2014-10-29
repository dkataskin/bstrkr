using System;

using bstrkr.core.spatial;
using bstrkr.core.utils;
using bstrkr.mvvm.views;

namespace bstrkr.mvvm.maps
{
	public interface IMapMarkerAnimator
	{
		event EventHandler<EventArgs> AnimationEnd;

		void Animate(IMapMarker marker, GeoPoint finalPosition, IGeoPointInterpolator interpolator, long duration);
	}
}