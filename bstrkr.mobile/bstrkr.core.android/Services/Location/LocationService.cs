using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.Gms.Common;
using Android.Gms.Common.Apis;
using Android.Gms.Location;
using Android.Locations;
using Android.OS;

using bstrkr.core.services.location;
using bstrkr.core.spatial;

using Cirrious.CrossCore.Droid;
using Cirrious.CrossCore.Exceptions;

namespace bstrkr.core.android.services.location
{
	public class LocationService : Java.Lang.Object, 
								   ILocationService, 
								   IGoogleApiClientConnectionCallbacks, 
								   IGoogleApiClientOnConnectionFailedListener, 
								   Android.Gms.Location.ILocationListener,
								   Android.Locations.ILocationListener
	{
		private readonly float _desiredAccuracy = 1000.0f;
		private readonly long _interval = 10000;
		private readonly float _displacement = 30;
		private readonly object _lockObject = new object();

		private readonly IMvxAndroidGlobals _androidGlobals;

		private LocationRequest _coarseLocationRequest;
		private IGoogleApiClient _googleAPIClient;
		private LocationManager _locationManager;

		private bool _located;

		public LocationService(IMvxAndroidGlobals androidGlobals)
		{
			_androidGlobals = androidGlobals;

			_coarseLocationRequest = LocationRequest.Create();
			_coarseLocationRequest.SetSmallestDisplacement(_displacement);
			_coarseLocationRequest.SetPriority(LocationRequest.PriorityBalancedPowerAccuracy);

			this.InitializeGoogleAPI();
		}

		public event EventHandler<LocationUpdatedEventArgs> LocationUpdated;

		public event EventHandler<LocationErrorEventArgs> LocatingFailed;

		public void StartUpdating()
		{
			if (GooglePlayServicesUtil.IsGooglePlayServicesAvailable(_androidGlobals.ApplicationContext) != ConnectionResult.Success)
			{
				throw new MvxException("Google Play Services are not available");
			}

			this.ConnectGoogleAPI();
		}

		public void StopUpdating()
		{
			LocationServices.FusedLocationApi.RemoveLocationUpdates(_googleAPIClient, this);
			this.DisconnectGoogleAPI();

			if (_locationManager != null)
			{
				_locationManager.RemoveUpdates(this);
			}
		}

		public void OnConnected(Bundle connectionHint)
		{
			LocationServices.FusedLocationApi.RequestLocationUpdates(_googleAPIClient, _coarseLocationRequest, this);

			Task.Delay(TimeSpan.FromSeconds(20))
				.ContinueWith(task => this.UseDifferentProviderIfNotLocated());
		}

		public void OnConnectionSuspended(int cause)
		{
		}

		public void OnConnectionFailed(ConnectionResult result)
		{
		}

		public void OnLocationChanged(Location location)
		{
			lock(_lockObject)
			{
				_located = true;
			}

			if (location != null)
			{
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

		private void InitializeGoogleAPI()
		{
			var queryResult = GooglePlayServicesUtil.IsGooglePlayServicesAvailable(_androidGlobals.ApplicationContext);

			if (queryResult == ConnectionResult.Success)
			{
				_googleAPIClient = new GoogleApiClientBuilder(_androidGlobals.ApplicationContext)
										.AddApi(LocationServices.Api)
										.AddConnectionCallbacks(this)
										.AddOnConnectionFailedListener(this)
										.Build();
			}
			else
			{
				var errorString = string.Format(
										"There is a problem with Google Play Services on this device: {0} - {1}", 
										queryResult, 
										GooglePlayServicesUtil.GetErrorString(queryResult));

				throw new MvxException(errorString);
			}
		}

		private void ConnectGoogleAPI()
		{
			if (!_googleAPIClient.IsConnectionCallbacksRegistered(this))
			{
				_googleAPIClient.RegisterConnectionCallbacks(this);
			}

			if (!_googleAPIClient.IsConnectionFailedListenerRegistered(this))
			{
				_googleAPIClient.RegisterConnectionFailedListener(this);
			}

			if (!_googleAPIClient.IsConnected || !_googleAPIClient.IsConnecting)
			{
				_googleAPIClient.Connect();
			}
		}

		private void DisconnectGoogleAPI()
		{
			if (_googleAPIClient != null && _googleAPIClient.IsConnected)
			{
				if (_googleAPIClient.IsConnectionCallbacksRegistered(this))
				{
					_googleAPIClient.UnregisterConnectionCallbacks(this);
				}

				if (_googleAPIClient.IsConnectionFailedListenerRegistered(this))
				{
					_googleAPIClient.UnregisterConnectionFailedListener(this);
				}

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

		private void UseDifferentProviderIfNotLocated()
		{
			lock(_lockObject)
			{
				if (!_located)
				{
					this.StopUpdating();

					var criteria = new Criteria();
					criteria.Accuracy = Accuracy.Coarse;
					criteria.PowerRequirement = Power.Low;
					criteria.AltitudeRequired = false;
					criteria.BearingRequired = false;
					criteria.SpeedRequired = false;

					_locationManager = _androidGlobals.ApplicationContext.GetSystemService(Context.LocationService) as LocationManager;
					var provider = _locationManager.GetBestProvider(criteria, true);

					if (!string.IsNullOrEmpty(provider))
					{
						_locationManager.RequestLocationUpdates(provider, 15000, 30.0f, this);
					}
				}
			}
		}
	}
}