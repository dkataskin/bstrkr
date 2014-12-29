using System;

using Android.OS;
using Android.Views;

using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Droid.FullFragging.Fragments;
using bstrkr.core;

namespace bstrkr.android.views
{
	public class PreferencesView : MvxFragment
	{
		public PreferencesView()
		{
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			var ignored = base.OnCreateView(inflater, container, savedInstanceState);

			this.Activity.Title = AppResources.preferences_view_title;

			return this.BindingInflate(Resource.Layout.fragment_preferences_view, null);
		}
	}
}