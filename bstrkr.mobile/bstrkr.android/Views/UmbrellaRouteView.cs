using System;

using Android.OS;
using Android.Runtime;
using Android.Views;

using bstrkr.core.android;
using bstrkr.core.android.views;
using bstrkr.mvvm.viewmodels;

using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Droid.FullFragging.Fragments;

namespace bstrkr.android.views
{
	[Register("bstrkr.android.views.UmbrellaRouteView")]
	public class UmbrellaRouteView : MvxFragment
	{
		public UmbrellaRouteView()
		{
			this.RetainInstance = true;
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			var ignored = base.OnCreateView(inflater, container, savedInstanceState);

			var dataContext = this.DataContext as UmbrellaRouteViewModel;
			(this.Activity as MvxActionBarActivity).SupportActionBar.Title = dataContext.Title;

			return this.BindingInflate(Resource.Layout.fragment_umbrellaroute_view, null);
		}
	}
}