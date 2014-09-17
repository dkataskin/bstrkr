using Android.Content;

using Cirrious.CrossCore;
using Cirrious.CrossCore.Platform;
using Cirrious.MvvmCross.Droid.Platform;
using Cirrious.MvvmCross.ViewModels;

using bstrkr.core.android.service;
using bstrkr.core.android.service.location;
using bstrkr.core.services.location;
using bstrkr.mvvm;

namespace bstrkr.android
{
    public class Setup : MvxAndroidSetup
    {
        public Setup(Context applicationContext) : base(applicationContext)
        {
        }

		protected override void InitializeFirstChance()
		{
			//Mvx.RegisterSingleton<IAndroidAppService>(new MainActivity());
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