using System;

using Google.Maps;

using MonoTouch.CoreLocation;

using bstrkr.core;

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
				Position = new CLLocationCoordinate2D(vehicle.Location.Latitude, vehicle.Location.Longitude),
				Flat = true,
				Rotation = vehicle.Heading,
				Icon
			};
		}
	}
}