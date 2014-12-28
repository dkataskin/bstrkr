using System;

using Android.OS;
using Android.Views;

using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Droid.FullFragging.Fragments;

using bstrkr.mvvm.viewmodels;

namespace bstrkr.android.views
{
	public class RoutesView : MvxFragment, IMenuItemOnMenuItemClickListener
	{
		public RoutesView()
		{
			this.RetainInstance = true;
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			this.SetHasOptionsMenu(true);

			var dataContext = this.DataContext as RoutesViewModel;
			this.Activity.ActionBar.Title = dataContext.Title;

			var ignored = base.OnCreateView(inflater, container, savedInstanceState);

			return this.BindingInflate(Resource.Layout.fragment_routes_view, null);
		}

		public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
		{
			inflater.Inflate(Resource.Menu.refresh, menu);
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