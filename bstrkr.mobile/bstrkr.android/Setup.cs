using Android.Content;

using Cirrious.CrossCore;
using Cirrious.CrossCore.Platform;
using Cirrious.MvvmCross.Droid.Platform;
using Cirrious.MvvmCross.ViewModels;

using bstrkr.core.android.services;
using bstrkr.core.android.services.location;
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
			Mvx.LazyConstructAndRegisterSingleton<ILocationService, SuperLocationService>();

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