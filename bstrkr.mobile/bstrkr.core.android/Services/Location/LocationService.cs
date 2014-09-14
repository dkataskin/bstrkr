using System;

using Android.Locations;

using bstrkr.core.services.location;
using System.Collections.Generic;
using System.Linq;
using Android.OS;

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
			var providerCriteria = new Criteria
			{
				Accuracy = Accuracy.Fine,
				PowerRequirement = Power.NoRequirement
			};

			var providerName = _locationManager.GetBestProvider(providerCriteria, true);

			if (!string.IsNullOrEmpty(providerName))
			{
				_locationProvider = providerName;
			}
			else
			{
				_locationProvider = string.Empty;
			}
		}

		public event EventHandler<LocationUpdatedEventArgs> LocationUpdated;

		public void StartUpdating()
		{
			_locationManager.RequestLocationUpdates(_locationProvider, 2000, 1, this);
		}

		public void StopUpdating()
		{
			_locationManager.RemoveUpdates(this);
		}

		public void OnLocationChanged(Location location)
		{
			this.RaiseLocationUpdatedEvent(location);
		}

		public void OnProviderDisabled(string provider)
		{
		}

		public void OnProviderEnabled(string provider)
		{
		}

		public void OnStatusChanged(string provider, Availability status, Bundle extras)
		{
		}

		public void Dispose()
		{
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