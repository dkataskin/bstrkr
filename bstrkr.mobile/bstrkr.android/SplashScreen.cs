using Android.App;
using Android.Content.PM;

using Cirrious.MvvmCross.Droid.Views;

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
    }
}