using System;

using Android.Locations;

using bstrkr.core.services.location;

namespace bstrkr.core.android.service.location
{
	public class LocationService : ILocationService
	{
		private LocationManager _locationManager;

		public LocationService()
		{

		}

		public event EventHandler<LocationUpdatedEventArgs> LocationUpdated;

		public void StartUpdating()
		{
			throw new NotImplementedException();
		}

		public void StopUpdating()
		{
			throw new NotImplementedException();
		}
	}
}

