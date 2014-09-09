using System;
using System.Collections.Generic;
using System.Linq;

using Cirrious.CrossCore;
using Cirrious.MvvmCross.Touch.Platform;
using Cirrious.MvvmCross.ViewModels;

using Google.Maps;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace bstrkr.ios
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the
	// User Interface of the application, as well as listening (and optionally responding) to
	// application events from iOS.
	[Register ("AppDelegate")]
	public partial class AppDelegate : MvxApplicationDelegate
	{
		private const string MapsApiKey = "AIzaSyAI2_0HkCS84Q5hWliLZQ64mCc81KZ61ME";
		// class-level declarations
		
		public override UIWindow Window { get; set; }

		public override void FinishedLaunching(UIApplication application)
		{
			// NOTE: Don't call the base implementation on a Model class
			// see http://docs.xamarin.com/guides/ios/application_fundamentals/delegates,_protocols,_and_events
			MapServices.ProvideAPIKey(MapsApiKey);

			var setup = new Setup(this, this.Window);
			setup.Initialize();

			var startup = Mvx.Resolve<IMvxAppStart>();
			startup.Start();
		}

		// This method is invoked when the application is about to move from active to inactive state.
		// OpenGL applications should use this method to pause.
		public override void OnResignActivation (UIApplication application)
		{
		}
		
		// This method should be used to release shared resources and it should store the application state.
		// If your application supports background exection this method is called instead of WillTerminate
		// when the user quits.
		public override void DidEnterBackground (UIApplication application)
		{
		}
		
		// This method is called as part of the transiton from background to active state.
		public override void WillEnterForeground (UIApplication application)
		{
		}
		
		// This method is called when the application is about to terminate. Save data, if needed.
		public override void WillTerminate (UIApplication application)
		{
		}
	}
}