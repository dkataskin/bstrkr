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
			var vehicleVM = item as VehicleViewModel;

			return new VehicleMarker 
			{
				Position = new CLLocationCoordinate2D(vehicleVM.Location.Latitude, vehicleVM.Location.Longitude),
				Flat = true,
				Rotation = vehicleVM.VehicleHeading,
				Icon = vehicleVM.Icon as UIImage
			};
		}
	}
}