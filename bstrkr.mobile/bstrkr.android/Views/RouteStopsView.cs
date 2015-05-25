﻿using System;
using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V13.App;
using Android.Support.V4.View;
using Android.Support.V7.Widget;
using Android.Views;

using bstrkr.core;
using bstrkr.core.android;
using bstrkr.mvvm.viewmodels;

using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Droid.FullFragging.Fragments;

using DK.Ostebaronen.Droid.ViewPagerIndicator;

namespace bstrkr.android.views
{
	[Register("bstrkr.android.views.RouteStopsView")]
	public class RouteStopsView : MvxFragment, 
								  IMenuItemOnMenuItemClickListener,
								  SearchView.IOnQueryTextListener,
								  View.IOnFocusChangeListener
	{
		private SearchView _searchView;

		public RouteStopsView()
		{
			this.RetainInstance = true;
		}

		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			this.SetHasOptionsMenu(true);
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			(this.Activity as MvxActionBarActivity).SupportActionBar.Title = AppResources.route_stops_view_title;

			var ignored = base.OnCreateView(inflater, container, savedInstanceState);

			var view = this.BindingInflate(Resource.Layout.fragment_stops_view, null);

			var pager = view.FindViewById<ViewPager>(Resource.Id.pager);
			var adaptor = new GenericFragmentPagerAdaptor(this.FragmentManager);

			adaptor.AddFragment("Close stops", new CloseRouteStopsView { DataContext = this.DataContext });
			adaptor.AddFragment("All stops", new AllRouteStopsView { DataContext = this.DataContext });

			pager.Adapter = adaptor;

			var indicator = view.FindViewById<TabPageIndicator>(Resource.Id.indicator);
			indicator.SetViewPager(pager);

			return view;
		}

		public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
		{
			inflater.Inflate(Resource.Menu.route_stops_view_menu, menu);

			var refreshItem = menu.FindItem(Resource.Id.menu_refresh);
			refreshItem.SetOnMenuItemClickListener(this);

			_searchView = menu.FindItem(Resource.Id.action_search).ActionView as SearchView;
			_searchView.QueryHint = AppResources.route_stops_view_search_hint;
			_searchView.SubmitButtonEnabled = false;
			_searchView.SetOnQueryTextListener(this);
			_searchView.SetOnQueryTextFocusChangeListener(this);
			_searchView.Iconified = true;
		}

		public override void OnDestroyView()
		{
			base.OnDestroyView();
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