using System;

using Android.OS;
using Android.Views;

using bstrkr.core;

using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Droid.FullFragging.Fragments;

namespace bstrkr.android.views
{
	public class RouteStopView : MvxFragment
	{
		public RouteStopView()
		{
			this.RetainInstance = true;
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			var ignored = base.OnCreateView(inflater, container, savedInstanceState);

			this.SetHasOptionsMenu(true);
			this.Activity.ActionBar.Title = AppResources.route_stop_view_title_format;

			return this.BindingInflate(Resource.Layout.fragment_stops_view, null);
		}
	}
}