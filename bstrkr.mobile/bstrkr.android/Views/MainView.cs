using Android.App;
using Android.Gms.Maps;
using Android.OS;

using Cirrious.MvvmCross.Droid.Views;
using bstrkr.core.android.services;

namespace bstrkr.android.Views
{
    [Activity(Label = "MainView")]
    public class MainView : MvxActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
        }

		protected override void OnViewModelSet()
		{
			base.OnViewModelSet();
			this.SetContentView(Resource.Layout.MainView);

			MapFragment mapFrag = (MapFragment)FragmentManager.FindFragmentById(Resource.Id.map);
			GoogleMap map = mapFrag.Map;
			if (map != null) 
			{
				map.MyLocationEnabled = true;
			}
		}
    }
}