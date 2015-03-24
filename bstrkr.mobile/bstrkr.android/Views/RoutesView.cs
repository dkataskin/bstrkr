using System;

using Android.OS;
using Android.Views;

using bstrkr.core;
using bstrkr.mvvm.viewmodels;

using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Droid.Fragging.Fragments;

namespace bstrkr.android.views
{
	/// <summary>
	/// List of all existing routes for current location.
	/// </summary>
	public class RoutesView : MvxFragment, IMenuItemOnMenuItemClickListener
	{
		public RoutesView()
		{
			this.RetainInstance = true;
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			this.HasOptionsMenu = true;

			this.Activity.ActionBar.Title = AppResources.routes_view_title;

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
	}
}