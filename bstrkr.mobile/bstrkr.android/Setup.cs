using System.Reflection;
using System.Resources;

using Android.Content;

using Cirrious.CrossCore;
using Cirrious.CrossCore.Converters;
using Cirrious.CrossCore.Platform;
using Cirrious.MvvmCross.Binding.Bindings.Target.Construction;
using Cirrious.MvvmCross.Droid.Platform;
using Cirrious.MvvmCross.Droid.Views;
using Cirrious.MvvmCross.Localization;
using Cirrious.MvvmCross.ViewModels;

using Xamarin;

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
using bstrkr.mvvm.localization;
using bstrkr.mvvm.views;
using bstrkr.providers;
using bstrkr.android.views;

namespace bstrkr.android
{
    public class Setup : MvxAndroidSetup
    {
        public Setup(Context applicationContext) : base(applicationContext)
        {
			Insights.Initialize("<xamarin_insights_key>", applicationContext);
        }

		protected override void InitializeFirstChance()
		{
			Mvx.RegisterSingleton(new ResxTextProvider(AppResources.ResourceManager));

			Mvx.LazyConstructAndRegisterSingleton<IConfigManager, ConfigManager>();
			Mvx.LazyConstructAndRegisterSingleton<ILocationService, SuperLocationService>();
			Mvx.LazyConstructAndRegisterSingleton<IAppResourceManager, AndroidAppResourceManager>();
			Mvx.LazyConstructAndRegisterSingleton<IBusTrackerLocationService, BusTrackerLocationService>();
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

//			var assembly = typeof(Resources.Strings.Strings).Assembly;
//			foreach (var res in assembly.GetManifestResourceNames()) 
//				System.Diagnostics.Debug.WriteLine("found resource: " + res);
        }

		protected override void FillTargetFactories(IMvxTargetBindingFactoryRegistry registry)
		{
			registry.RegisterCustomBindingFactory<MonoDroidGoogleMapsView>(
																		"Zoom", 
																		mapView => new MapViewZoomTargetBinding(mapView));

			base.FillTargetFactories(registry);
		}

		protected override IMvxAndroidViewPresenter CreateViewPresenter()
		{
			return Mvx.Resolve<ICustomPresenter>();
		}

		protected override void FillValueConverters(IMvxValueConverterRegistry registry)
		{
			base.FillValueConverters(registry);
			registry.AddOrOverwrite("Language", new MvxLanguageConverter());
		}
    }
}