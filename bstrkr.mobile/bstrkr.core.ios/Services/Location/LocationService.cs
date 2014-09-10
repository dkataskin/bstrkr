using System;

using MonoTouch.CoreLocation;
using MonoTouch.UIKit;

using bstrkr.core.services.location;
using System.Linq;

namespace bstrkr.core.ios.service.location
{
	public class LocationService : ILocationService
	{
		private CLLocationManager _locationManager;

		public LocationService()
		{
			_locationManager = new CLLocationManager();
			_locationManager.DesiredAccuracy = CLLocation.AccuracyHundredMeters;

			// handle the updated location method and update the UI
			if (UIDevice.CurrentDevice.CheckSystemVersion (6, 0)) 
			{
				_locationManager.LocationsUpdated += (object sender, CLLocationsUpdatedEventArgs args) => 
				{
					this.RaiseLocationUpdatedEvent(args.Locations.Last().Coordinate);
				};
			} 
			else 
			{
				// this won't be called on iOS 6 (deprecated)
				_locationManager.UpdatedLocation += (object sender, CLLocationUpdatedEventArgs args) => 
				{
					this.RaiseLocationUpdatedEvent(args.NewLocation.Coordinate);
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

		private void RaiseLocationUpdatedEvent(CLLocationCoordinate2D coord)
		{
			if (this.LocationUpdated != null)
			{
				this.LocationUpdated(this, new LocationUpdatedEventArgs(coord.Latitude, coord.Longitude));
			}
		}
	}
}