using System;

using Google.Maps;

using bstrkr.mvvm.views;

namespace bstrkr.ios.views
{
	public class MonoTouchGoogleMapsView : IMapView
	{
		private readonly MapView _mapView;

		public MonoTouchGoogleMapsView(MapView mapView)
		{
			_mapView = mapView;
		}
	}
}