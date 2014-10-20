using System;

using bstrkr.core.spatial;

namespace bstrkr.mvvm.views
{
	public interface IMapMarker
	{
		IMapView MapView { get; set; }

		GeoPoint Location { get; set; }
	}
}