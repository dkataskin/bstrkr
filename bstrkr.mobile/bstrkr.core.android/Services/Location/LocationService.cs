using System;
using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.Gms.Common;
using Android.Gms.Location;
using Android.Locations;
using Android.OS;

using bstrkr.core.services.location;

namespace bstrkr.core.android.services.location
{
	public class LocationService : ILocationService, IGooglePlayServicesClientConnectionCallbacks, 
	IGooglePlayServicesClientOnConnectionFailedListener, Android.Gms.Location.ILocationListener
	{
		private Activity _mainActivity;
		private LocationClient _locationClient;

		public LocationService(IAndroidAppService androidAppService)
		{
			_mainActivity = androidAppService.GetMainActivity();
			_locationClient = new LocationClient(_mainActivity, this, this);
		}

		public event EventHandler<LocationUpdatedEventArgs> LocationUpdated;

		public void StartUpdating()
		{
			_locationClient.Connect();
		}

		public void StopUpdating()
		{
			if (_locationClient.IsConnected)
			{
				_locationClient.RemoveLocationUpdates(this);
				_locationClient.Disconnect();
			}
		}

		public void OnConnected(Bundle connectionHint)
		{
			var locationRequest = new LocationRequest();

			locationRequest.SetPriority(100);
			locationRequest.SetFastestInterval(500);
			locationRequest.SetInterval(1000);

			_locationClient.RequestLocationUpdates(locationRequest, this);
		}

		public void OnDisconnected()
		{
		}

		public void OnConnectionFailed(ConnectionResult result)
		{
		}

		public void OnLocationChanged(Location location)
		{
			this.RaiseLocationUpdatedEvent(location);
		}

		public void Dispose()
		{
		}

		public IntPtr Handle 
		{
			get { return _mainActivity.Handle; }
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