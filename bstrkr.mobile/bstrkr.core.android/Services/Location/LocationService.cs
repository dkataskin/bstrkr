using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Common;
using Android.Gms.Common.Apis;
using Android.Gms.Location;
using Android.Gms.Maps;
using Android.Locations;
using Android.OS;
//using Android.Support.V4.Content;

using bstrkr.core.services.location;
using bstrkr.core.spatial;

using Cirrious.CrossCore.Droid;
using Cirrious.CrossCore.Droid.Platform;
using Cirrious.CrossCore.Exceptions;

namespace bstrkr.core.android.services.location
{
	public class LocationService : Java.Lang.Object, 
								   ILocationService, 
								   GoogleApiClient.IConnectionCallbacks, 
								   GoogleApiClient.IOnConnectionFailedListener, 
								   Android.Gms.Location.ILocationListener,
								   Android.Locations.ILocationListener,
								   ILocationSource
	{
		private const int InstallGooglePlayServicesId = 1000;

		private readonly float _displacement = 30;

		private readonly IMvxAndroidGlobals _androidGlobals;
		private readonly IMvxAndroidCurrentTopActivity _topActivity;

		private LocationRequest _locationRequest;
		private GoogleApiClient _googleAPIClient;
		private LocationManager _locationManager;
		private ILocationSourceOnLocationChangedListener _mapsListener;

		public LocationService(IMvxAndroidGlobals androidGlobals, IMvxAndroidCurrentTopActivity topActivity)
		{
			_androidGlobals = androidGlobals;
			_topActivity = topActivity;

			_locationRequest = LocationRequest.Create();
			_locationRequest.SetSmallestDisplacement(_displacement);
			_locationRequest.SetPriority(LocationRequest.PriorityLowPower);

			this.InitializeGoogleAPI();
		}

		public event EventHandler<LocationUpdatedEventArgs> LocationUpdated;

		public event EventHandler<LocationErrorEventArgs> LocatingFailed;

		public void StartUpdating()
		{
			var apiAvailability = GoogleApiAvailability.Instance;
			var queryResult = apiAvailability.IsGooglePlayServicesAvailable(_androidGlobals.ApplicationContext);
			if (queryResult != ConnectionResult.Success)
			{
				if (apiAvailability.IsUserResolvableError(queryResult))
				{
					apiAvailability.ShowErrorDialogFragment(_topActivity.Activity, queryResult, InstallGooglePlayServicesId);
				}
				else
				{
					// throw new MvxException("Google Play Services are not available");
					return;
				}
			}

			this.ConnectGoogleAPI();
		}

		public GeoPoint GetLastLocation()
		{
			var lastLocation = LocationServices.FusedLocationApi.GetLastLocation(_googleAPIClient);

			if (lastLocation == null)
			{
				return GeoPoint.Empty;
			}

			return new GeoPoint(lastLocation.Latitude, lastLocation.Longitude);
		}

		public void StopUpdating()
		{
			if (_googleAPIClient != null && _googleAPIClient.IsConnected)
			{
				LocationServices.FusedLocationApi.RemoveLocationUpdates(_googleAPIClient, this);
			}

			this.DisconnectGoogleAPI();

			if (_locationManager != null)
			{
				_locationManager.RemoveUpdates(this);
			}
		}

		public void OnConnected(Bundle connectionHint)
		{
			var lastLocation = LocationServices.FusedLocationApi.GetLastLocation(_googleAPIClient);
			if (lastLocation == null)
			{
				_locationRequest.SetPriority(LocationRequest.PriorityHighAccuracy);
				_locationRequest.SetNumUpdates(1);
			}
			else
			{
				this.OnLocationChanged(lastLocation);
			}

			LocationServices.FusedLocationApi.RequestLocationUpdates(_googleAPIClient, _locationRequest, this);
		}

		public void OnConnectionSuspended(int cause)
		{
		}

		public void OnConnectionFailed(ConnectionResult result)
		{
		}

		public void OnLocationChanged(Location location)
		{
			if (location != null)
			{
				var mapsListener = _mapsListener;
				if (mapsListener != null)
				{
					mapsListener.OnLocationChanged(location);
				}

				this.RaiseLocationUpdatedEvent(location);
			}
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

		public void Activate(ILocationSourceOnLocationChangedListener listener)
		{
			_mapsListener = listener;
		}

		public void Deactivate()
		{
			_mapsListener = null;
		}

		private void InitializeGoogleAPI()
		{
			var apiAvailability = GoogleApiAvailability.Instance;
			var queryResult = apiAvailability.IsGooglePlayServicesAvailable(_androidGlobals.ApplicationContext);
			if (queryResult == ConnectionResult.Success)
			{
				_googleAPIClient = new GoogleApiClient.Builder(_androidGlobals.ApplicationContext)
										.AddApi(LocationServices.API)
										.AddConnectionCallbacks(this)
										.AddOnConnectionFailedListener(this)
										.Build();
			}
			else
			{
				var errorString = string.Format(
										"There is a problem with Google Play Services on this device: {0} - {1}", 
										queryResult, 
										apiAvailability.GetErrorString(queryResult));

				throw new MvxException(errorString);
			}
		}

		private void ConnectGoogleAPI()
		{
			_googleAPIClient.RegisterConnectionCallbacks(this);
			_googleAPIClient.RegisterConnectionFailedListener(this);

			if (!_googleAPIClient.IsConnected || !_googleAPIClient.IsConnecting)
			{
				_googleAPIClient.Connect();
			}
		}

		private void DisconnectGoogleAPI()
		{
			if (_googleAPIClient != null && _googleAPIClient.IsConnected)
			{
				_googleAPIClient.UnregisterConnectionCallbacks(this);
				_googleAPIClient.UnregisterConnectionFailedListener(this);

				_googleAPIClient.Disconnect();
			}
		}

		private void RaiseLocationUpdatedEvent(Location location)
		{
			if (this.LocationUpdated != null)
			{
				this.LocationUpdated(this, new LocationUpdatedEventArgs(location.Latitude, location.Longitude));
			}
		}

		private void RaiseLocationUpdatedEvent(double lat, double lon)
		{
			if (this.LocationUpdated != null)
			{
				this.LocationUpdated(this, new LocationUpdatedEventArgs(lat, lon));
			}
		}
	}
}