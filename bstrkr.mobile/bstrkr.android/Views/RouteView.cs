using System;

using Android.OS;
using Android.Views;

using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Droid.FullFragging.Fragments;

using bstrkr.mvvm.viewmodels;
using bstrkr.core;

namespace bstrkr.android.views
{
	public class RoutesView : MvxFragment
	{
		public RoutesView()
		{
			this.RetainInstance = true;
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			this.SetHasOptionsMenu(true);

			this.Activity.ActionBar.Title = AppResources.routes_view_title;

			var ignored = base.OnCreateView(inflater, container, savedInstanceState);

			return this.BindingInflate(Resource.Layout.fragment_routes_view, null);
		}
	}
}