using System.IO;

using Cirrious.CrossCore;
using Cirrious.CrossCore.Platform;
using Cirrious.MvvmCross.Touch.Platform;
using Cirrious.MvvmCross.ViewModels;

using MonoTouch.UIKit;

using bstrkr.core.config;
using bstrkr.core.ios.config;
using bstrkr.core.ios.service.location;
using bstrkr.core.ios.services.resources;
using bstrkr.core.services.location;
using bstrkr.core.services.resources;
using bstrkr.mvvm;

namespace bstrkr.ios
{
	public class Setup : MvxTouchSetup
	{
		public Setup(MvxApplicationDelegate applicationDelegate, UIWindow window)
            : base(applicationDelegate, window)
		{
		}

		protected override void InitializeFirstChance()
		{
			Mvx.LazyConstructAndRegisterSingleton<IConfigManager, ConfigManager>();
			Mvx.LazyConstructAndRegisterSingleton<ILocationService, LocationService>();
			Mvx.LazyConstructAndRegisterSingleton<IResourceManager, ResourceManager>();

			base.InitializeFirstChance();
		}

		protected override IMvxApplication CreateApp()
		{
			return new BusTrackerApp();
		}
		
        protected override IMvxTrace CreateDebugTrace()
        {
            return new DebugTrace();
        }
	}
}