using System;

using bstrkr.core.spatial;

namespace bstrkr.mvvm.maps
{
	public interface IMarkerLocationAnimationRunner
	{
		void RunAnimation(GeoPoint finalLocation, TimeSpan timespan);
	}
}