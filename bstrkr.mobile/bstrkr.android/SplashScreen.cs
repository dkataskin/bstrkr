using Android.App;
using Android.Content.PM;
using Android.OS;

using Cirrious.MvvmCross.Droid.Views;

using Xamarin;

namespace bstrkr.android
{
    [Activity(
        Label = "BusTracker",
        MainLauncher = true,
        Icon = "@drawable/ic_launcher",
        Theme = "@style/Theme.Splash",
        NoHistory = true,
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class SplashScreen : MvxSplashScreenActivity
    {
        public SplashScreen() : base(Resource.Layout.page_splash_screen)
        {
        }

        protected override void OnCreate(Bundle bundle)
        {
            Insights.Initialize("<your_key_here>", this.Application.ApplicationContext, true);
            base.OnCreate(bundle);
        }
    }
}