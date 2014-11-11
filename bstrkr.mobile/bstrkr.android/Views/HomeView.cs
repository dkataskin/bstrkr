using System;
using System.Linq;

using Android.App;
using Android.Gms.Common;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Views;

using Cirrious.CrossCore;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Binding.Droid.Views;
using Cirrious.MvvmCross.Droid.Fragging;
using Cirrious.MvvmCross.Droid.Fragging.Fragments;
using Cirrious.MvvmCross.Droid.Views;
using Cirrious.MvvmCross.ViewModels;

using Xamarin;

using bstrkr.core.android.extensions;
using bstrkr.core.android.presenters;
using bstrkr.core.android.services;
using bstrkr.core.android.views;
using bstrkr.core.consts;
using bstrkr.mvvm.converters;
using bstrkr.mvvm.maps;
using bstrkr.mvvm.viewmodels;
using bstrkr.mvvm.views;

namespace bstrkr.android.views
{
    [Activity(Label = "HomeView")]
	public class HomeView : MvxFragmentActivity, IFragmentHost
    {
		private DrawerLayout _drawer;
		private MyActionBarDrawerToggle _drawerToggle;
		private string _drawerTitle;
		private string _title;
		private MvxListView _drawerList;

		public bool Show(MvxViewModelRequest request)
		{
			try
			{
				var homeViewModel = this.ViewModel as HomeViewModel;
				MvxFragment fragment = null;
				var title = string.Empty;
				var section = homeViewModel.GetSectionForViewModelType(request.ViewModelType);

				switch (section)
				{
					case MenuSection.Map:
						{
							if (this.SupportFragmentManager.FindFragmentById(Resource.Id.content_frame) as MapView != null)
							{
								return true;
							}

							fragment = new MapView();
							title = "Map";
						}
						break;

					case MenuSection.Routes:
						{
							if (this.SupportFragmentManager.FindFragmentById(Resource.Id.content_frame) as RoutesView != null)
							{
								return true;
							}

							fragment = new RoutesView();
							title = "Routes";
						}
						break;

					case MenuSection.Settings:
						{
							if (this.SupportFragmentManager.FindFragmentById(Resource.Id.content_frame) as SettingsView != null)
							{
								return true;
							}

							fragment = new SettingsView();
							title = "Profile";
						}
						break;
				}

				var loaderService = Mvx.Resolve<IMvxViewModelLoader>();
				var viewModel = loaderService.LoadViewModel(request, null /* saved state */);

				fragment.ViewModel = viewModel;

				// TODO - replace this with extension method when available

				//Normally we would do this, but we already have it
				this.SupportFragmentManager.BeginTransaction().Replace(Resource.Id.content_frame, fragment).Commit();

				var menuItem = homeViewModel.MenuItems.First(x => x.Id == (int)section);
				_drawerList.SetItemChecked(homeViewModel.MenuItems.IndexOf(menuItem), true);
				this.ActionBar.Title = _title = title;

				_drawer.CloseDrawer(this._drawerList);

				return true;
			}
			finally
			{
				_drawer.CloseDrawer(_drawerList); 
			}
		}

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
			this.SetContentView(Resource.Layout.page_home_view);

			_title = _drawerTitle = this.Title;
			_drawer = this.FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
			_drawerList = this.FindViewById<MvxListView>(Resource.Id.left_drawer);

			_drawer.SetDrawerShadow(Resource.Drawable.drawer_shadow_dark, (int)GravityFlags.Start);

			this.ActionBar.SetDisplayHomeAsUpEnabled(true);
			this.ActionBar.SetHomeButtonEnabled(true);

			//DrawerToggle is the animation that happens with the indicator next to the
			//ActionBar icon.
			_drawerToggle = new MyActionBarDrawerToggle(
													this,
													_drawer,
													Resource.Drawable.ic_drawer_light,
													Resource.String.drawer_open,
													Resource.String.drawer_close);

			_drawerToggle.DrawerClosed += delegate
			{
				this.ActionBar.Title = _title;
				this.InvalidateOptionsMenu();
			};

			_drawerToggle.DrawerOpened += delegate
			{
				this.ActionBar.Title = _drawerTitle;
				this.InvalidateOptionsMenu();
			};

			_drawer.SetDrawerListener(_drawerToggle);

			this.RegisterForDetailsRequests();

			if (null == savedInstanceState)
			{
				var homeViewModel = this.ViewModel as HomeViewModel;
				homeViewModel.SelectMenuItemCommand.Execute(homeViewModel.MenuItems[0]);
			}
        }

		private void RegisterForDetailsRequests()
		{
			var customPresenter = Mvx.Resolve<ICustomPresenter>();
			customPresenter.Register(typeof(SettingsViewModel), this);
			customPresenter.Register(typeof(RoutesViewModel), this);
			customPresenter.Register(typeof(AboutViewModel), this);
			customPresenter.Register(typeof(MapViewModel), this);
		}
    }
}