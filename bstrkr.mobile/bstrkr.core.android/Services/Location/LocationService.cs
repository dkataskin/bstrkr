using System;
using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.Content;
using Android.Gms.Common;
using Android.Gms.Common.Apis;
using Android.Gms.Location;
using Android.Locations;
using Android.OS;

using Cirrious.CrossCore.Droid;
using Cirrious.CrossCore.Exceptions;

using bstrkr.core.services.location;
using bstrkr.core.spatial;

namespace bstrkr.core.android.services.location
{
	public class LocationService : Java.Lang.Object, 
								   ILocationService, 
								   IGoogleApiClientConnectionCallbacks, 
								   IGoogleApiClientOnConnectionFailedListener, 
								   Android.Gms.Location.ILocationListener
	{
		private readonly float _desiredAccuracy = 1000.0f;
		private readonly long _interval = 10000;
		private readonly float _displacement = 30;
		private readonly IMvxAndroidGlobals _androidGlobals;

		private LocationRequest _fineLocationRequest;
		private LocationRequest _coarseLocationRequest;
		private IGoogleApiClient _googleAPIClient;

		public LocationService(IMvxAndroidGlobals androidGlobals)
		{
			_androidGlobals = androidGlobals;

			_fineLocationRequest = LocationRequest.Create();
			_fineLocationRequest.SetInterval(_interval);
			_fineLocationRequest.SetFastestInterval(1000);
			_fineLocationRequest.SetPriority(LocationRequest.PriorityBalancedPowerAccuracy);

			_coarseLocationRequest = LocationRequest.Create();
			_coarseLocationRequest.SetSmallestDisplacement(_displacement);
			_coarseLocationRequest.SetFastestInterval(1000);
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
		}

		public void OnConnected(Bundle connectionHint)
		{
			LocationServices.FusedLocationApi.RequestLocationUpdates(_googleAPIClient, _fineLocationRequest, this);
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
				if (location.Accuracy <= _desiredAccuracy)
				{
					LocationServices.FusedLocationApi.RemoveLocationUpdates(_googleAPIClient, this);
					LocationServices.FusedLocationApi.RequestLocationUpdates(_googleAPIClient, _coarseLocationRequest, this);
				}

				this.RaiseLocationUpdatedEvent(location);
			}
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
	}
}