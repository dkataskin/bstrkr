using System;

using Android.OS;
using Android.Preferences;
using Android.Views;

using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Droid.FullFragging.Fragments;

using bstrkr.core;

namespace bstrkr.android.views
{
	public class PreferencesView : PreferenceFragment
	{
		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			this.AddPreferencesFromResource(Resource.Xml.preferences);

			this.Activity.ActionBar.Title = AppResources.preferences_view_title;
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			var view = base.OnCreateView(inflater, container, savedInstanceState);
			view.SetBackgroundResource(Resource.Color.background_holo_dark);

			return view;
		}
	}
}