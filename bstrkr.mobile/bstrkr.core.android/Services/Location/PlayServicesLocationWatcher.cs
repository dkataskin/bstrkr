//using System;
//
//using Android.App;
//using Android.Content;
//using Android.Gms.Common;
//using Android.Gms.Location;
//using Android.OS;
//
//using Cirrious.CrossCore;
//using Cirrious.CrossCore.Droid;
//using Cirrious.CrossCore.Exceptions;
//using Cirrious.MvvmCross.Plugins.Location;
//using Cirrious.MvvmCross.Plugins.Location.Droid;
//
//namespace Services.Location
//{
//	public class PlayServicesLocationWatcher : MvxLocationWatcher, IMvxLocationReceiver
//	{
//		private Context _context;
//		private LocationClient _locationClient;
//		private LocationRequest _locationRequest;
//		private readonly PlayConnectionCallbacksListener _connectionCallBacks;
//		private readonly PlayConnectionFailedListener _connectionFailed;
//		private readonly MvxLocationListener _locationListener;
//
//		private Context Context
//		{
//			get
//			{
//				return _context ??
//					(_context = Mvx.Resolve<IMvxAndroidGlobals>().ApplicationContext);
//			}
//		}
//
//		public PlayServicesLocationWatcher()
//		{
//			EnsureStopped();
//
//			_connectionCallBacks = new PlayConnectionCallbacksListener(this);
//			_connectionFailed = new PlayConnectionFailedListener(this);
//			_locationListener = new MvxLocationListener(this);
//		}
//
//		public override MvxGeoLocation CurrentLocation
//		{
//			get
//			{
//				if (_locationClient == null || _locationRequest == null)
//					throw new MvxException("Location Client not started");
//
//				var androidLocation = _locationClient.LastLocation;
//				return androidLocation == null ? null : CreateLocation(androidLocation);
//			}
//		}
//
//		protected override void PlatformSpecificStart(MvxLocationOptions options)
//		{
//			if (_locationClient != null)
//				throw new MvxException("You cannot start MvxLocation service more than once");
//
//			if (GooglePlayServicesUtil.IsGooglePlayServicesAvailable(Context) != ConnectionResult.Success)
//			{
//				throw new MvxException("Google Play Services are not available");
//			}
//
//			_locationRequest = LocationRequest.Create();
//			_locationRequest.SetInterval((long)options.TimeBetweenUpdates.TotalMilliseconds);
//			_locationRequest.SetSmallestDisplacement(options.MovementThresholdInM);
//			_locationRequest.SetFastestInterval(1000);
//
//			_locationRequest.SetPriority(options.Accuracy == MvxLocationAccuracy.Fine
//				? LocationRequest.PriorityHighAccuracy
//				: LocationRequest.PriorityBalancedPowerAccuracy);
//
//			_locationClient = new LocationClient(Context, _connectionCallBacks, _connectionFailed);
//			_locationClient.Connect();
//		}
//
//		protected override void PlatformSpecificStop()
//		{
//			EnsureStopped();
//		}
//
//		private void EnsureStopped()
//		{
//			if (_locationClient == null) return;
//
//			_locationClient.RemoveLocationUpdates(_locationListener);
//			_locationClient.Disconnect();
//			_locationClient = null;
//			_locationRequest = null;
//		}
//
//		private static MvxGeoLocation CreateLocation(Location androidLocation)
//		{
//			var position = new MvxGeoLocation {Timestamp = androidLocation.Time.FromMillisecondsUnixTimeToUtc()};
//			var coords = position.Coordinates;
//
//			if (androidLocation.HasAltitude)
//				coords.Altitude = androidLocation.Altitude;
//
//			if (androidLocation.HasBearing)
//				coords.Heading = androidLocation.Bearing;
//
//			coords.Latitude = androidLocation.Latitude;
//			coords.Longitude = androidLocation.Longitude;
//			if (androidLocation.HasSpeed)
//				coords.Speed = androidLocation.Speed;
//			if (androidLocation.HasAccuracy)
//			{
//				coords.Accuracy = androidLocation.Accuracy;
//			}
//
//			return position;
//		}
//
//		public void OnLocationChanged(Location androidLocation)
//		{
//			if (androidLocation == null)
//			{
//				MvxTrace.Trace("Android: Null location seen");
//				return;
//			}
//
//			if (AlmostEqual(androidLocation.Latitude, double.MaxValue)
//				|| AlmostEqual(androidLocation.Longitude, double.MaxValue))
//			{
//				MvxTrace.Trace("Android: Invalid location seen");
//				return;
//			}
//
//			MvxGeoLocation location;
//			try
//			{
//				location = CreateLocation(androidLocation);
//			}
//			catch (ThreadAbortException)
//			{
//				throw;
//			}
//			catch (Exception ex)
//			{
//				MvxTrace.Trace("Android: Exception seen in converting location " + ex.ToLongString());
//				return;
//			}
//
//			SendLocation(location);
//		}
//
//		public static bool AlmostEqual(double a, double b)
//		{
//			return Math.Abs(a - b) < Math.Abs(a)*0.000001; //10 cm precision at equator
//		}
//
//		public void OnConnected(Bundle p0)
//		{
//			_locationClient.RequestLocationUpdates(_locationRequest, _locationListener);
//		}
//
//		public void OnDisconnected()
//		{
//			//TODO
//		}
//
//		public void OnConnectionFailed(ConnectionResult p0)
//		{
//			if (p0.HasResolution)
//			{
//				try
//				{
//					Mvx.TaggedTrace("OnConnectionFailed()", "Launching Resolution for ConnectionResult with ErrorCode: {0}", p0.ErrorCode.ToString());
//					var intent = new Intent();
//					var receiver = new ConnectionFailedPendingIntentReceiver();
//					receiver.ResultOk += ok => {
//						if (ok)
//							_locationClient.Connect();
//					};
//					p0.Resolution.Send(Context, 0, intent, receiver, null);
//				}
//				catch (PendingIntent.CanceledException ex)
//				{
//					Mvx.TaggedTrace("OnConnectionFailed()", "Resolution for ConnectionResult Cancelled! Exception: {0}", ex);
//				}
//			}
//		}
//
//		public class ConnectionFailedPendingIntentReceiver 
//			: Java.Lang.Object
//		, PendingIntent.IOnFinished
//		{
//			public Action<bool> ResultOk;
//
//			public void OnSendFinished(PendingIntent pendingIntent, Intent intent, Result resultCode, string resultData,
//				Bundle resultExtras)
//			{
//				var res = ResultOk;
//				if (res != null)
//				{
//					res(resultCode == Result.Ok);
//				}
//			}
//		}
//}
//
