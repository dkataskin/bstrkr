using Android.App;
using Android.OS;
using Cirrious.MvvmCross.Droid.Views;

namespace bstrkr.android.Views
{
    [Activity(Label = "MainView")]
    public class MainView : MvxActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.MainView);
        }
    }
}