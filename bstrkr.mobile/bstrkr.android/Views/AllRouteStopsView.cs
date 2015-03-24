using Android.OS;
using Android.Views;

using bstrkr.android;

using Cirrious.MvvmCross.Binding.Droid.BindingContext;

using Android.App;
using Cirrious.MvvmCross.Droid.Fragging.Fragments;

namespace bstrkr.android.views
{
	public class AllRouteStopsView : MvxFragment
	{
		public AllRouteStopsView()
		{
			this.RetainInstance = true;
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			var ignored = base.OnCreateView(inflater, container, savedInstanceState);

			return this.BindingInflate(Resource.Layout.fragment_allstops_view, null);
		}
	}
}