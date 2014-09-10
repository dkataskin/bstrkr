using Cirrious.CrossCore;

using Cirrious.CrossCore.Platform;
using Cirrious.MvvmCross.Touch.Platform;
using Cirrious.MvvmCross.ViewModels;

using MonoTouch.UIKit;

using bstrkr.core.ios.service.location;
using bstrkr.core.services.location;
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
			Mvx.LazyConstructAndRegisterSingleton<ILocationService, LocationService>();
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