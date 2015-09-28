using System;

using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Views;

using bstrkr.core;
using bstrkr.core.android;
using bstrkr.core.android.views;
using bstrkr.mvvm.viewmodels;

using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Droid.FullFragging.Fragments;

namespace bstrkr.android.views
{
	[Register("bstrkr.android.views.PreferencesView")]
	public class PreferencesView : MvxFragment
	{
		public PreferencesView()
		{
			this.RetainInstance = true;
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			this.SetHasOptionsMenu(true);

			var ignored = base.OnCreateView(inflater, container, savedInstanceState);

			(this.Activity as MvxAppCompatActivity).SupportActionBar.Title = AppResources.preferences_view_title;

			return this.BindingInflate(Resource.Layout.fragment_preferences_view, null);
		}

		public override void OnPrepareOptionsMenu(IMenu menu)
		{
			menu.Clear();
			base.OnPrepareOptionsMenu(menu);
		}

		public override void OnStop()
		{
			base.OnStop();
			(this.ViewModel as PreferencesViewModel).SavePreferencesCommand.Execute();
		}
	}
}