using System;

using Android;
using Android.Content.PM;
using Android.Support.V4.App;
using Android.Support.V4.Content;

using Cirrious.CrossCore.Droid;
using Cirrious.CrossCore.Droid.Platform;

namespace bstrkr.android.util
{
	public class PermissionsHelper
	{
		private readonly IMvxAndroidGlobals _globals;
		private readonly IMvxAndroidCurrentTopActivity _activityRef;

		private readonly string [] _permissionsLocation = 
		{
			Manifest.Permission.AccessCoarseLocation,
			Manifest.Permission.AccessFineLocation
		};

		public PermissionsHelper(IMvxAndroidGlobals globals, IMvxAndroidCurrentTopActivity activity)
		{
			_globals = globals;
			_activityRef = activity;
		}

		public void CheckLocationPermission(Action permissionGranted, Action permissionNotGranted)
		{
			const string permission = Manifest.Permission.AccessFineLocation;
			if (ContextCompat.CheckSelfPermission(_globals.ApplicationContext, permission) == (int)Permission.Granted)
			{
				permissionGranted.Invoke();
				return;
			}

			if (ActivityCompat.ShouldShowRequestPermissionRationale(_activityRef.Activity, permission))
			{
			}

			ActivityCompat.RequestPermissions(_activityRef.Activity, _permissionsLocation, RequestLocationId); 
		}
	}
}