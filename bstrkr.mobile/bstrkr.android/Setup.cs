using Android.Content;

using Cirrious.CrossCore;
using Cirrious.CrossCore.Platform;
using Cirrious.MvvmCross.Binding.Bindings.Target.Construction;
using Cirrious.MvvmCross.Droid.Platform;
using Cirrious.MvvmCross.ViewModels;

using bstrkr.core.android.config;
using bstrkr.core.android.services;
using bstrkr.core.android.services.location;
using bstrkr.core.android.services.resources;
using bstrkr.core.config;
using bstrkr.core.services.location;
using bstrkr.core.services.resources;
using bstrkr.mvvm;
using bstrkr.mvvm.views;
using bstrkr.android.views;
using Xamarin;

namespace bstrkr.android
{
    public class Setup : MvxAndroidSetup
    {
        public Setup(Context applicationContext) : base(applicationContext)
        {
			Insights.Initialize("<your_key_here>", applicationContext);
        }

		protected override void InitializeFirstChance()
		{
			Mvx.LazyConstructAndRegisterSingleton<IConfigManager, ConfigManager>();
			Mvx.LazyConstructAndRegisterSingleton<ILocationService, SuperLocationService>();
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

		protected override void FillTargetFactories(IMvxTargetBindingFactoryRegistry registry)
		{
			registry.RegisterCustomBindingFactory<MonoDroidGoogleMapsView>(
																		"Zoom", 
																		mapView => new MapViewZoomTargetBinding(mapView));

			base.FillTargetFactories(registry);
		}
    }
}