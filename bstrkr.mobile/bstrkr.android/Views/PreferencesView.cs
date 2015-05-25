using System;

using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Views;

using bstrkr.core;
using bstrkr.core.android;

using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Droid.FullFragging.Fragments;

namespace bstrkr.android.views
{
	[Register("bstrkr.android.views.PreferencesView")]
	public class PreferencesView : MvxFragment
	{
		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			(this.Activity as MvxActionBarActivity).SupportActionBar.Title = AppResources.preferences_view_title;
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			var view = base.OnCreateView(inflater, container, savedInstanceState);
			view.SetBackgroundResource(Resource.Color.background_holo_dark);

			return view;
		}
	}
}