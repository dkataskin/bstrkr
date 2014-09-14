using System;

using Android.App;
using Android.Content;
using Android.GoogleMaps;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using bstrkr.core.android.service;
using bstrkr.providers;

namespace bstrkr.android
{
	[Activity (Label = "bstrkr.android", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : MapActivity, IAndroidAppService
	{
		public Activity GetMainActivity()
		{
			return this;
		}

		protected override void OnCreate (Bundle bundle)
		{
			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);
		}

		protected override bool IsRouteDisplayed { get { false; } }
	}
}


