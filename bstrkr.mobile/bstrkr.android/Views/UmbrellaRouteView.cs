using System;

using Android.OS;
using Android.Views;

using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Droid.FullFragging.Fragments;

using bstrkr.mvvm.viewmodels;

namespace bstrkr.android.views
{
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
			this.Activity.ActionBar.Title = dataContext.Title;
			this.Activity.ActionBar.SetDisplayHomeAsUpEnabled(true);

			return this.BindingInflate(Resource.Layout.fragment_umbrellaroute_view, null);
		}
	}
}