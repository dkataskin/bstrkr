using System;

using Android.Locations;

using bstrkr.core.services.location;
using System.Collections.Generic;
using System.Linq;

namespace bstrkr.core.android.service.location
{
	public class LocationService : ILocationService, ILocationListener
	{
		private string _locationProvider;
		private LocationManager _locationManager;

		public LocationService(IAndroidAppService androidAppService)
		{
			var mainActivity = androidAppService.GetMainActivity();

			_locationManager = mainActivity.GetSystemService(LocationService) as LocationManager;
			Criteria criteriaForLocationService = new Criteria
			{
				Accuracy = Accuracy.Fine
			};

			IList<string> acceptableLocationProviders = _locationManager.GetProviders(criteriaForLocationService, true);

			if (acceptableLocationProviders.Any())
			{
				_locationProvider = acceptableLocationProviders.First();
			}
			else
			{
				_locationProvider = String.Empty;
			}
		}

		public event EventHandler<LocationUpdatedEventArgs> LocationUpdated;

		public void StartUpdating()
		{
			_locationManager.RequestLocationUpdates(_locationProvider, 2000, 1, this);
		}

		public void StopUpdating()
		{
			//_locationManager.r
		}

		public void OnLocationChanged(Location location)
		{
			throw new NotImplementedException();
		}

		public void OnProviderDisabled(string provider)
		{
			throw new NotImplementedException();
		}

		public void OnProviderEnabled(string provider)
		{
			throw new NotImplementedException();
		}

		public void OnStatusChanged(string provider, Availability status, Android.OS.Bundle extras)
		{
			throw new NotImplementedException();
		}

		public void Dispose()
		{
			throw new NotImplementedException();
		}

		public IntPtr Handle 
		{
			get 
			{
				throw new NotImplementedException();
			}
		}

		private void RaiseLocationUpdatedEvent(Location location)
		{
			if (this.LocationUpdated != null)
			{
				this.LocationUpdated(this, new LocationUpdatedEventArgs(location.Latitude, location.Longitude));
			}
		}
	}
}