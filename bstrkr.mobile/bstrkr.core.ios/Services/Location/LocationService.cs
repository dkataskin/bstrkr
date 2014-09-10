using System;

using MonoTouch.CoreLocation;
using MonoTouch.UIKit;

using bstrkr.core.services.location;

namespace bstrkr.core.ios.service.location
{
	public class LocationService : ILocationService
	{
		private CLLocationManager _locationManager;

		public LocationService()
		{
			_locationManager = new CLLocationManager();

			// uncomment this if you want to use the delegate pattern:
			//locationDelegate = new LocationDelegate (mainScreen);
			//iPhoneLocationManager.Delegate = locationDelegate;

			// you can set the update threshold and accuracy if you want:
			//iPhoneLocationManager.DistanceFilter = 10; // move ten meters before updating
			//iPhoneLocationManager.HeadingFilter = 3; // move 3 degrees before updating

			_locationManager.DesiredAccuracy = 100;
			// you can also use presets, which simply evalute to a double value:
			//iPhoneLocationManager.DesiredAccuracy = CLLocation.AccuracyNearestTenMeters;

			// handle the updated location method and update the UI
			if (UIDevice.CurrentDevice.CheckSystemVersion (6, 0)) 
			{
				_locationManager.LocationsUpdated += (object sender, CLLocationsUpdatedEventArgs e) => 
				{
					//UpdateLocation (mainScreen, e.Locations [e.Locations.Length - 1]);
				};
			} 
			else 
			{
				// this won't be called on iOS 6 (deprecated)
				_locationManager.UpdatedLocation += (object sender, CLLocationUpdatedEventArgs e) => 
				{
					//UpdateLocation (mainScreen, e.NewLocation);
				};
			}
		}

		public event EventHandler<LocationUpdatedEventArgs> LocationUpdated;

		public void StartUpdating()
		{
			if (CLLocationManager.LocationServicesEnabled)
			{
				_locationManager.StartUpdatingLocation();
			}
		}

		public void StopUpdating()
		{
			if (CLLocationManager.LocationServicesEnabled)
			{
				_locationManager.StopUpdatingLocation();
			}
		}
	}
}