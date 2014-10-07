using System;

using Google.Maps;

using MonoTouch.CoreLocation;
using MonoTouch.UIKit;

using bstrkr.core;
using bstrkr.mvvm.viewmodels;

namespace bstrkr.ios.views
{
	public class VehicleMarkerManager : MvxMarkerManager
	{
		public VehicleMarkerManager(MapView mapView) : base(mapView)
		{
		}

		protected override VehicleMarker CreateMarker(object item)
		{
			return new VehicleMarker(item as VehicleViewModel);
		}
	}
}