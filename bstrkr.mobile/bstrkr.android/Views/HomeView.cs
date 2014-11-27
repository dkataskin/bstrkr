using System;
using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.Content.PM;
using Android.Content.Res;
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
	[Activity(Label = "Home", 
			  LaunchMode = LaunchMode.SingleTop, 
			  Icon = "@drawable/ic_launcher")]
	public class HomeView : MvxFragmentActivity, IFragmentHost
    {
		private readonly IDictionary<MenuSection, string> _menu2Tag = new Dictionary<MenuSection, string>
		{
			{ MenuSection.Map, "map_view_frag" },
			{ MenuSection.Routes, "routes_view_frag" },
			{ MenuSection.Preferences, "prefs_view_frag" },
			{ MenuSection.About, "about_view_frag" }
		};

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
				fragment = this.SupportFragmentManager.FindFragmentByTag(_menu2Tag[section]) as MvxFragment;

				switch (section)
				{
					case MenuSection.Map:
						if (this.IsCurrentFragment<MapView>())
						{
							return true;
						}

						if (fragment == null)
						{
							fragment = new MapView();
						}

						title = Resources.GetString(Resource.String.map_view_title);
						break;

					case MenuSection.Routes:
						if (this.IsCurrentFragment<RoutesView>())
						{
							return true;
						}

						if (fragment == null)
						{
							fragment = new RoutesView();
						}

						title = Resources.GetString(Resource.String.routes_view_title);
						break;

					case MenuSection.Preferences:
						if (this.IsCurrentFragment<PreferencesView>())
						{
							return true;
						}

						if (fragment == null)
						{
							fragment = new PreferencesView();
						}

						title = Resources.GetString(Resource.String.prefs_view_title);
						break;

					case MenuSection.About:
						var loaderService = Mvx.Resolve<IMvxViewModelLoader>();
						var viewModel = loaderService.LoadViewModel(request, null /* saved state */);
						var dialog = new AboutView();
						dialog.ViewModel = viewModel;
						title = Resources.GetString(Resource.String.about_view_title);
//						var menuItem = homeViewModel.MenuItems.First(x => x.Id == (int)section);
//						_drawerList.SetItemChecked(homeViewModel.MenuItems.IndexOf(menuItem), true);
//						this.ActionBar.Title = _title = title;
//
//						_drawer.CloseDrawer(_drawerList);

						dialog.Show(this.SupportFragmentManager, null);

						return true;
						break;
				}

				if (fragment.ViewModel == null)
				{
					var loaderService = Mvx.Resolve<IMvxViewModelLoader>();
					var viewModel = loaderService.LoadViewModel(request, null /* saved state */);

					fragment.ViewModel = viewModel;
				}

				// Normally we would do this, but we already have it
				this.SupportFragmentManager.BeginTransaction()
										   .Replace(Resource.Id.content_frame, fragment, _menu2Tag[section])
										   .Commit();

				var menuItem = homeViewModel.MenuItems.First(x => x.Id == (int)section);
				_drawerList.SetItemChecked(homeViewModel.MenuItems.IndexOf(menuItem), true);
				this.ActionBar.Title = _title = title;

				_drawer.CloseDrawer(_drawerList);

				return true;
			}
			finally
			{
				_drawer.CloseDrawer(_drawerList); 
			}
		}

		protected override void OnPostCreate(Bundle savedInstanceState)
		{
			base.OnPostCreate(savedInstanceState);
			_drawerToggle.SyncState();
		}

		public override void OnConfigurationChanged(Configuration newConfig)
		{
			base.OnConfigurationChanged(newConfig);
			_drawerToggle.OnConfigurationChanged(newConfig);
		}

		public override bool OnCreateOptionsMenu(IMenu menu)
		{
			//MenuInflater.Inflate(Resource.Menu.main, menu);
			return base.OnCreateOptionsMenu(menu);
		}

		public override bool OnPrepareOptionsMenu(IMenu menu)
		{
			var drawerOpen = _drawer.IsDrawerOpen(_drawerList);

			//when open don't show anything
			for (int i = 0; i < menu.Size(); i++)
			{
				menu.GetItem(i).SetVisible(!drawerOpen);
			}

			return base.OnPrepareOptionsMenu(menu);
		}

		public override bool OnOptionsItemSelected(IMenuItem item)
		{
			if (_drawerToggle.OnOptionsItemSelected(item))
			{
				return true;
			}

			return base.OnOptionsItemSelected(item);
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
			customPresenter.Register(typeof(PreferencesViewModel), this);
			customPresenter.Register(typeof(RoutesViewModel), this);
			customPresenter.Register(typeof(AboutViewModel), this);
			customPresenter.Register(typeof(MapViewModel), this);
		}

		private bool IsCurrentFragment<TView>() where TView : MvxFragment
		{
			return this.SupportFragmentManager.FindFragmentById(Resource.Id.content_frame) as TView != null;
		}
    }
}