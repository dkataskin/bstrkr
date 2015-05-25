using System;

using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Views;

using bstrkr.android;

using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Droid.FullFragging.Fragments;

namespace bstrkr.android.views
{
	[Register("bstrkr.android.views.CloseRouteStopsView")]
	public class CloseRouteStopsView : MvxFragment
	{
		public CloseRouteStopsView()
		{
			this.RetainInstance = true;
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			var ignored = base.OnCreateView(inflater, container, savedInstanceState);

			return this.BindingInflate(Resource.Layout.fragment_closeroutestops_view, null);
		}
	}
}