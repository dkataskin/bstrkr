using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using bstrkr.core.providers;

namespace bstrkr.android
{
	[Activity (Label = "bstrkr.android", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : Activity
	{
		int count = 2;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			// Get our button from the layout resource,
			// and attach an event to it
			Button button = FindViewById<Button> (Resource.Id.myButton);
			
			button.Click += delegate {
				var provider = new Bus13LiveDataProvider("http://bus13.ru/php");
				var task = provider.GetRoutesAsync().ConfigureAwait(false);
				var result = task.GetAwaiter().GetResult();
				button.Text = string.Format ("{0} clicks!", count++);
			};
		}
	}
}


