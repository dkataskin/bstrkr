using System;

using Android.OS;
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
	/// <summary>
	/// List of all existing routes for current location.
	/// </summary>
	[Register("bstrkr.android.views.RoutesView")]
	public class RoutesView : MvxFragment, IMenuItemOnMenuItemClickListener
	{
		public RoutesView()
		{
			this.RetainInstance = true;
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			this.SetHasOptionsMenu(true);

			(this.Activity as MvxAppCompatActivity).SupportActionBar.Title = AppResources.routes_view_title;

			var ignored = base.OnCreateView(inflater, container, savedInstanceState);

			return this.BindingInflate(Resource.Layout.fragment_routes_view, null);
		}

		public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
		{
			inflater.Inflate(Resource.Menu.routes_view_menu, menu);
			var refreshItem = menu.FindItem(Resource.Id.menu_refresh);
			refreshItem.SetOnMenuItemClickListener(this);
		}

		public bool OnMenuItemClick(IMenuItem item)
		{
			if (item.ItemId == Resource.Id.menu_refresh)
			{
				(this.ViewModel as RoutesViewModel).RefreshCommand.Execute();
				return true;
			}

			return false;
		}

		public override void OnResume()
		{
			base.OnResume();
			(this.ViewModel as RoutesViewModel).RefreshCommand.Execute();
		}
	}
}