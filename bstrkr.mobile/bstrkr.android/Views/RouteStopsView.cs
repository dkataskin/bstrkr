﻿using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;

using bstrkr.core;
using bstrkr.mvvm.viewmodels;

using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Droid.FullFragging.Fragments;

namespace bstrkr.android.views
{
	public class RouteStopsView : MvxFragment, 
								  IMenuItemOnMenuItemClickListener,
								  Android.Widget.SearchView.IOnQueryTextListener,
								  Android.Views.View.IOnFocusChangeListener
	{
		private SearchView _searchView;
		private ActionBar.Tab _closeStopsTab;
		private ActionBar.Tab _allStopsTab;
		private ActionBarNavigationMode _actionBarNavigationMode;

		public RouteStopsView()
		{
			this.RetainInstance = true;
		}

		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			this.SetHasOptionsMenu(true);

			_closeStopsTab = this.Activity.ActionBar.NewTab();
			_closeStopsTab.SetText("Close to me");
			_closeStopsTab.TabSelected += (s, a) =>
			{
				(this.DataContext as RouteStopsViewModel).ProximityFilter = true;
			};

			_allStopsTab = this.Activity.ActionBar.NewTab();
			_allStopsTab.TabSelected += (s, a) =>
			{
				(this.DataContext as RouteStopsViewModel).ProximityFilter = false;
			};

			_allStopsTab.SetText("All stops");
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			this.Activity.ActionBar.Title = AppResources.route_stops_view_title;

			_actionBarNavigationMode = this.Activity.ActionBar.NavigationMode;

			this.Activity.ActionBar.NavigationMode = ActionBarNavigationMode.Tabs;

			this.Activity.ActionBar.AddTab(_closeStopsTab);
			this.Activity.ActionBar.AddTab(_allStopsTab);

			var ignored = base.OnCreateView(inflater, container, savedInstanceState);

			return this.BindingInflate(Resource.Layout.fragment_stops_view, null);
		}

		public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
		{
			inflater.Inflate(Resource.Menu.route_stops_view_menu, menu);

			var refreshItem = menu.FindItem(Resource.Id.menu_refresh);
			refreshItem.SetOnMenuItemClickListener(this);

			_searchView = menu.FindItem(Resource.Id.action_search).ActionView as SearchView;
			_searchView.SetQueryHint(AppResources.route_stops_view_search_hint);
			_searchView.SubmitButtonEnabled = false;
			_searchView.SetOnQueryTextListener(this);
			_searchView.SetOnQueryTextFocusChangeListener(this);
			_searchView.Iconified = false;
		}

		public override void OnDestroyView()
		{
			base.OnDestroyView();

			this.Activity.ActionBar.RemoveTab(_closeStopsTab);
			this.Activity.ActionBar.RemoveTab(_allStopsTab);

			this.Activity.ActionBar.NavigationMode = _actionBarNavigationMode;
		}

		public bool OnQueryTextChange(string newText)
		{
			(this.DataContext as RouteStopsViewModel).FilterSting = newText;
			return true;
		}

		public bool OnQueryTextSubmit(string query)
		{
			return false;
		}

		public void OnFocusChange(View view, bool hasFocus)
		{
			if (view is SearchView && !hasFocus)
			{
				(this.DataContext as RouteStopsViewModel).FilterSting = string.Empty;
			}
		}

		public bool OnMenuItemClick(IMenuItem item)
		{
			if (item.ItemId == Resource.Id.menu_refresh)
			{
				(this.ViewModel as RouteStopsViewModel).RefreshCommand.Execute();
				return true;
			}

			return false;
		}
	}
}