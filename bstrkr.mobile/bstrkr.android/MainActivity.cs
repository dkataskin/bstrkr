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
	public class MainActivity : Activity, IAndroidAppService
	{
		public Activity GetMainActivity()
		{
			return this;
		}
	}
}