using System;

using Android.OS;
using Android.Views;

using bstrkr.core;

using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Droid.FullFragging.Fragments;

namespace bstrkr.android.views
{
	public class LicensesView : MvxFragment
	{
		public LicensesView()
		{
			this.RetainInstance = true;
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			var ignored = base.OnCreateView(inflater, container, savedInstanceState);

			this.Activity.ActionBar.Title = AppResources.licenses_view_title;

			return this.BindingInflate(Resource.Layout.fragment_licenses_view, null);
		}
	}
}