using System;

using Android.OS;
using Android.Runtime;
using Android.Views;

using bstrkr.core;
using bstrkr.core.android;
using bstrkr.core.android.views;
using bstrkr.mvvm;
using bstrkr.mvvm.viewmodels;

using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Droid.FullFragging.Fragments;

namespace bstrkr.android.views
{
	[Register("bstrkr.android.views.RouteStopView")]
	public class RouteStopView : MvxFragment, IMenuItemOnMenuItemClickListener
	{
		public RouteStopView()
		{
			this.RetainInstance = true;
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			var ignored = base.OnCreateView(inflater, container, savedInstanceState);

			this.SetHasOptionsMenu(true);

			var vm = this.DataContext as RouteStopViewModel;
			(this.Activity as MvxAppCompatActivity).SupportActionBar.Title = string.Format(
													AppResources.route_stop_view_title_format,
													vm.Name,
													vm.Description);

			return this.BindingInflate(Resource.Layout.fragment_routestop_view, null);
		}

		public bool OnMenuItemClick(IMenuItem item)
		{
			if (item.ItemId == Resource.Id.menu_showonmap)
			{
				(this.ViewModel as RouteStopViewModel).ShowOnMapCommand.Execute();
				return true;
			}

			return false;
		}

		public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
		{
			menu.Clear();

			inflater.Inflate(Resource.Menu.route_stop_view_menu, menu);

			var showOnMapMenuItem = menu.FindItem(Resource.Id.menu_showonmap);
			showOnMapMenuItem.SetOnMenuItemClickListener(this);
		}

		public override void OnDestroyView()
		{
			base.OnDestroyView();

			if (this.DataContext != null && this.DataContext is ICleanable)
			{
				(this.DataContext as ICleanable).CleanUp();
			}
		}
	}
}