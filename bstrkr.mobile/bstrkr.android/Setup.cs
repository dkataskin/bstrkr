using System.Globalization;
using System.Reflection;
using System.Resources;

using System.Threading;

using Android.Content;

using bstrkr.android.views;
using bstrkr.core;
using bstrkr.core.android.config;
using bstrkr.core.android.presenters;
using bstrkr.core.android.services;
using bstrkr.core.android.services.location;
using bstrkr.core.android.services.resources;
using bstrkr.core.config;
using bstrkr.core.services.location;
using bstrkr.core.services.resources;
using bstrkr.mvvm;
using bstrkr.mvvm.bindings;
using bstrkr.mvvm.views;
using bstrkr.providers;

using Cirrious.CrossCore;
using Cirrious.CrossCore.Converters;
using Cirrious.CrossCore.Platform;
using Cirrious.MvvmCross.Binding.Bindings.Target.Construction;
using Cirrious.MvvmCross.Droid.Platform;
using Cirrious.MvvmCross.Droid.Views;
using Cirrious.MvvmCross.Localization;
using Cirrious.MvvmCross.ViewModels;

using Xamarin;

namespace bstrkr.android
{
    public class Setup : MvxAndroidSetup
    {
        public Setup(Context applicationContext) : base(applicationContext)
        {
        }

		protected override void InitializeFirstChance()
		{
			Mvx.LazyConstructAndRegisterSingleton<IConfigManager, ConfigManager>();
			Mvx.LazyConstructAndRegisterSingleton<ILocationService, LocationService>();
			Mvx.LazyConstructAndRegisterSingleton<IAreaPositioningService, AreaPositioningService>();
			Mvx.LazyConstructAndRegisterSingleton<IBusTrackerLocationService, BusTrackerLocationService>();
			Mvx.LazyConstructAndRegisterSingleton<IAppResourceManager, AndroidAppResourceManager>();
			Mvx.LazyConstructAndRegisterSingleton<ILiveDataProviderFactory, DefaultLiveDataProviderFactory>();
			Mvx.RegisterSingleton<ICustomPresenter>(new CustomPresenter());

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

			registry.RegisterCustomBindingFactory<MonoDroidGoogleMapsView>(
																"VisibleRegion",
																mapView => new MapViewVisibleRegionTargetBinding(mapView));

			base.FillTargetFactories(registry);
		}

		protected override IMvxAndroidViewPresenter CreateViewPresenter()
		{
			return Mvx.Resolve<ICustomPresenter>();
		}

		protected override void FillValueConverters(IMvxValueConverterRegistry registry)
		{
			base.FillValueConverters(registry);
		}
    }
}