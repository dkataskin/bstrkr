using System;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V13.App;
using Android.Support.V4.View;

using Android.Views;
using Android.Widget;

using bstrkr.core;
using bstrkr.mvvm.viewmodels;

using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Droid.FullFragging.Fragments;
using DK.Ostebaronen.Droid.ViewPagerIndicator;
using System.Linq;

namespace bstrkr.android.views
{
	public class RouteStopsView : MvxFragment, 
								  IMenuItemOnMenuItemClickListener,
								  SearchView.IOnQueryTextListener,
								  View.IOnFocusChangeListener
	{
		private IMenuItem _searchViewItem;
		private SearchView _searchView;
//		private ActionBar.Tab _closeStopsTab;
//		private ActionBar.Tab _allStopsTab;

		private Fragment _fr1;
		private Fragment _fr2;

		private ActionBarNavigationMode _actionBarNavigationMode;

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
			this.Activity.ActionBar.Title = AppResources.route_stops_view_title;

			_actionBarNavigationMode = this.Activity.ActionBar.NavigationMode;
//			this.Activity.ActionBar.NavigationMode = ActionBarNavigationMode.Tabs;

			var ignored = base.OnCreateView(inflater, container, savedInstanceState);

			var view = this.BindingInflate(Resource.Layout.fragment_stops_view, null);

			var pager = view.FindViewById<ViewPager>(Resource.Id.pager);
			var adaptor = new GenericFragmentPagerAdaptor(this.FragmentManager);

			adaptor.AddFragment("Close stops", _fr1 = new CloseRouteStopsView { DataContext = this.DataContext });
			adaptor.AddFragment("All stops", _fr2 = new AllRouteStopsView { DataContext = this.DataContext });

			pager.Adapter = adaptor;
//			pager.SetOnPageChangeListener(
//				new ViewPageListenerForActionBar(this.Activity.ActionBar, position => _searchViewItem.CollapseActionView()));

			var indicator = view.FindViewById<TabPageIndicator>(Resource.Id.indicator);
			indicator.SetViewPager(pager);

//			_closeStopsTab = pager.GetViewPageTab(this.Activity.ActionBar, "Close to me");
//			_allStopsTab = pager.GetViewPageTab(this.Activity.ActionBar, "All stops");

//			this.Activity.ActionBar.AddTab(_closeStopsTab);
//			this.Activity.ActionBar.AddTab(_allStopsTab);

			return view;
		}

		public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
		{
			inflater.Inflate(Resource.Menu.route_stops_view_menu, menu);

			var refreshItem = menu.FindItem(Resource.Id.menu_refresh);
			refreshItem.SetOnMenuItemClickListener(this);

			_searchViewItem = menu.FindItem(Resource.Id.action_search);
			_searchView = menu.FindItem(Resource.Id.action_search).ActionView as SearchView;
			_searchView.SetQueryHint(AppResources.route_stops_view_search_hint);
			_searchView.SubmitButtonEnabled = false;
			_searchView.SetOnQueryTextListener(this);
			_searchView.SetOnQueryTextFocusChangeListener(this);
			_searchView.Iconified = true;
		}

		public override void OnDestroyView()
		{
			base.OnDestroyView();

//			this.FragmentManager.PopBackStack();
//			this.FragmentManager.PopBackStack();

//			this.Activity.ActionBar.RemoveTab(_closeStopsTab);
//			this.Activity.ActionBar.RemoveTab(_allStopsTab);

//			this.Activity.ActionBar.NavigationMode = _actionBarNavigationMode;

//			_closeStopsTab.Dispose();
//			_allStopsTab.Dispose();
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

	public class GenericFragmentPagerAdaptor : FragmentPagerAdapter, IDisposable
	{
		private IDictionary<string, Fragment> _fragments = new Dictionary<string, Fragment>();


		public GenericFragmentPagerAdaptor(FragmentManager fragmentManager)
			: base(fragmentManager)
		{
		}

		public override int Count
		{
			get { return _fragments.Keys.Count; }
		}

		public override Fragment GetItem(int position)
		{
			return _fragments.Values.ElementAt(position);
		}

		public void AddFragment(string title, Fragment fragment)
		{
			_fragments.Add(title, fragment);
			NotifyDataSetChanged();
		}

		public override Java.Lang.ICharSequence GetPageTitleFormatted(int position)
		{
			return new Java.Lang.String(_fragments.Keys.ElementAt(position));
		}

		protected override void Dispose(bool disposing)
		{
//			base.Dispose(disposing);

			foreach (var item in _fragments)
			{
//				item.Dispose();
			}

//			_fragments.Clear();
		}
	}

//	public class ViewPageListenerForActionBar : ViewPager.SimpleOnPageChangeListener
//	{
//		private ActionBar _bar;
//		private Action<int> _onPageSelectedCallback;
//
//		public ViewPageListenerForActionBar(ActionBar bar)
//		{
//			_bar = bar;
//		}
//
//		public ViewPageListenerForActionBar(ActionBar bar, Action<int> onPageSelectedCallback) : this(bar)
//		{
//			_onPageSelectedCallback = onPageSelectedCallback;
//		}
//
//		public override void OnPageSelected(int position)
//		{
//			_bar.SetSelectedNavigationItem(position);
//			if (_onPageSelectedCallback != null)
//			{
//				_onPageSelectedCallback(position);
//			}
//		}
//	}
//
//	public static class ViewPagerExtensions
//	{
//		public static ActionBar.Tab GetViewPageTab(this ViewPager viewPager, ActionBar actionBar, string name)
//		{
//			var tab = actionBar.NewTab();
//			tab.SetText(name);
//			tab.TabSelected += (o, e) =>
//			{
//				viewPager.SetCurrentItem(actionBar.SelectedNavigationIndex, false);
//			};
//
//			return tab;
//		}
//	}
}