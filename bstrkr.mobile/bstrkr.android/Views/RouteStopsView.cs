using System;

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
		public RouteStopsView()
		{
			this.RetainInstance = true;
		}

		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			this.SetHasOptionsMenu(true);

			this.Activity.ActionBar.Title = AppResources.route_stops_view_title;
			this.Activity.ActionBar.NavigationMode = ActionBarNavigationMode.Tabs;

			var closeStopsTab = this.Activity.ActionBar.NewTab();
			closeStopsTab.SetText("Close to me");
			closeStopsTab.TabSelected += (s, a) =>
			{
			};

			this.Activity.ActionBar.AddTab(closeStopsTab);

			var allStopsTab = this.Activity.ActionBar.NewTab();
			allStopsTab.TabSelected += (s, a) =>
			{
			};

			allStopsTab.SetText("All stops");
			this.Activity.ActionBar.AddTab(allStopsTab);
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			var ignored = base.OnCreateView(inflater, container, savedInstanceState);

			return this.BindingInflate(Resource.Layout.fragment_stops_view, null);
		}

		public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
		{
			inflater.Inflate(Resource.Menu.route_stops_view_menu, menu);

			var refreshItem = menu.FindItem(Resource.Id.menu_refresh);
			refreshItem.SetOnMenuItemClickListener(this);

			var searchView = menu.FindItem(Resource.Id.action_search).ActionView as SearchView;
			searchView.Iconified = false;
			searchView.SetQueryHint(AppResources.route_stops_view_search_hint);
			searchView.SubmitButtonEnabled = false;
			searchView.SetOnQueryTextListener(this);
			searchView.SetOnQueryTextFocusChangeListener(this);
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