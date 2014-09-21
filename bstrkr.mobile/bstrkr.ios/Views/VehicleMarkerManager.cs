using System;

using Google.Maps;

using bstrkr.core;

using MonoTouch.CoreLocation;

namespace bstrkr.ios.views
{
	public class VehicleMarkerManager : MvxMarkerManager
	{
		public VehicleMarkerManager(MapView mapView) : base(mapView)
		{
		}

		protected override VehicleMarker CreateMarker(object item)
		{
			var vehicle = item as Vehicle;

			return new VehicleMarker 
			{
				Position = new CLLocationCoordinate2D(vehicle.Location.Latitude, vehicle.Location.Longitude)
			};
		}
	}
}