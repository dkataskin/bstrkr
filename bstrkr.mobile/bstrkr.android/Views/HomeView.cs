using System;
using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Gms.Common;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Views;
using Android.Widget;

using bstrkr.core;
using bstrkr.core.android.extensions;
using bstrkr.core.android.presenters;
using bstrkr.core.android.services;
using bstrkr.core.android.views;
using bstrkr.core.consts;
using bstrkr.core.context;
using bstrkr.core.utils;
using bstrkr.mvvm.converters;
using bstrkr.mvvm.maps;
using bstrkr.mvvm.messages;
using bstrkr.mvvm.viewmodels;
using bstrkr.mvvm.views;

using Chance.MvvmCross.Plugins.UserInteraction;

using Cirrious.CrossCore;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Binding.Droid.Views;
using Cirrious.MvvmCross.Droid.FullFragging;
using Cirrious.MvvmCross.Droid.FullFragging.Fragments;
using Cirrious.MvvmCross.Droid.Views;
using Cirrious.MvvmCross.Plugins.Messenger;
using Cirrious.MvvmCross.ViewModels;

using Xamarin;

namespace bstrkr.android.views
{
	[Activity(Label = "Home", 
			  LaunchMode = LaunchMode.SingleTop, 
			  Icon = "@drawable/ic_launcher",
			  ScreenOrientation = ScreenOrientation.Portrait)]
	public class HomeView : MvxActivity, IFragmentHost
    {
		private DrawerLayout _drawer;
		private MyActionBarDrawerToggle _drawerToggle;
		private MvxListView _drawerList;
		private MenuSection _currentSection;

		private IMvxMessenger _messenger;

		private static IDictionary<Type, string> _frag2tag = new Dictionary<Type, string>();

		public bool Show(MvxViewModelRequest request)
		{
			try
			{
				var loaderService = Mvx.Resolve<IMvxViewModelLoader>();
				var homeViewModel = this.ViewModel as HomeViewModel;
				Fragment fragment = null;

				var section = homeViewModel.GetSectionForViewModelType(request.ViewModelType);
				if (section == MenuSection.Unknown)
				{
					this.Navigate(request, loaderService);
					return true;
				}

				switch (section)
				{
					case MenuSection.Map:
						_drawerList.SetItemChecked(0, true);

						var map = this.FragmentManager.FindFragmentById<MapView>(Resource.Id.mapView);
						if (map.ViewModel == null)
						{
							map.ViewModel = loaderService.LoadViewModel(request, null /* saved state */);
						}

						var transaction = this.FragmentManager.BeginTransaction();
						foreach (var viewType in _frag2tag.Keys) 
						{
							if (viewType != typeof(MapView))
							{
								var fragmentToRemove = this.FragmentManager.FindFragmentByTag(_frag2tag[viewType]);
								if (fragmentToRemove != null)
								{
									transaction.Remove(fragmentToRemove);
								}
							}
						}

						this.FragmentManager.PopBackStackImmediate(null, PopBackStackFlags.None | PopBackStackFlags.Inclusive);
						transaction.Commit();
						return true;

					case MenuSection.Routes:
						fragment = this.FindFragment<RoutesView>() ?? new RoutesView();
						_currentSection = section;
						break;

					case MenuSection.RouteStops:
						fragment = this.FindFragment<RouteStopsView>() ?? new RouteStopsView();
						_currentSection = section;
						break;

					case MenuSection.Preferences:
						fragment = this.FindFragment<PreferencesView>() ?? new PreferencesView();
						_currentSection = section;
						break;

					case MenuSection.Licenses:
						fragment = this.FindFragment<LicensesView>() ?? new LicensesView();
						_currentSection = section;
						break;

					case MenuSection.About:
						var menuItem1 = homeViewModel.MenuItems.First(x => x.Id == (int)_currentSection);
						var position = homeViewModel.MenuItems.IndexOf(menuItem1);
						var aboutViewModel = loaderService.LoadViewModel(request, null) as AboutViewModel;
						Mvx.Resolve<IUserInteraction>().Alert(
												aboutViewModel.AboutText,
												() => _drawerList.SetItemChecked(position, true),
												AppResources.about_view_title,
												AppResources.ok);

						return true;
				}
//
//				if (fragment.Tag == _tag && _tag != null)
//				{
//					return true;
//				}
				IMvxFragmentView mvxFragmentView = null;
				if (fragment is IMvxFragmentView)
				{
					mvxFragmentView = fragment as IMvxFragmentView;
				}
				if (mvxFragmentView != null && mvxFragmentView.ViewModel == null)
				{
					mvxFragmentView.ViewModel = loaderService.LoadViewModel(request, null /* saved state */);
				}

				if (!_frag2tag.ContainsKey(fragment.GetType()))
				{
					_frag2tag[fragment.GetType()] = Guid.NewGuid().ToString();
				}

				this.FragmentManager.BeginTransaction()
									.Replace(Resource.Id.content_frame, fragment, _frag2tag[fragment.GetType()])
								   	.AddToBackStack(null)
								   	.Commit();

				var menuItem = homeViewModel.MenuItems.First(x => x.Id == (int)section);
				_drawerList.SetItemChecked(homeViewModel.MenuItems.IndexOf(menuItem), true);

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

			if (item.ItemId == Android.Resource.Id.Home)
			{
				if (this.NavigateBack())
				{
					return true;
				}
			}

			return base.OnOptionsItemSelected(item);
		}

        protected override void OnCreate(Bundle savedInstanceState)
        {
			var context = this.ApplicationContext;
			BusTrackerAppContext.Version = context.PackageManager
												  .GetPackageInfo(context.PackageName, 0)
												  .VersionName;

            base.OnCreate(savedInstanceState);
			this.SetContentView(Resource.Layout.page_home_view);

			_messenger = Mvx.Resolve<IMvxMessenger>();

			_drawer = this.FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
			_drawerList = this.FindViewById<MvxListView>(Resource.Id.left_drawer);

			_drawer.SetDrawerShadow(Resource.Drawable.drawer_shadow_dark, (int)GravityFlags.Start);

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
				this.InvalidateOptionsMenu();
			};

			_drawerToggle.DrawerOpened += delegate
			{
				this.InvalidateOptionsMenu();
			};

			_drawer.SetDrawerListener(_drawerToggle);

			this.RegisterForDetailsRequests();

			this.DisableDrawer();

			this.ActionBar.SetDisplayHomeAsUpEnabled(false);
			this.ActionBar.SetHomeButtonEnabled(false);
			this.ActionBar.Title = AppResources.refreshing;

			_messenger.SubscribeOnMainThread<LocationUpdateMessage>(msg => 
			{
				this.EnableDrawer();

				this.ActionBar.SetDisplayHomeAsUpEnabled(true);
				this.ActionBar.SetHomeButtonEnabled(true);
				this.ActionBar.Title = AppResources.map_view_title;
			});

			if (null == savedInstanceState)
			{
				this.ShowMap();
			}
        }

		public override void OnBackPressed()
		{
			if (!this.NavigateBack())
			{
				base.OnBackPressed();
			}
		}

		private void RegisterForDetailsRequests()
		{
			var customPresenter = Mvx.Resolve<ICustomPresenter>();
			customPresenter.Register(typeof(PreferencesViewModel), this);
			customPresenter.Register(typeof(RoutesViewModel), this);
			customPresenter.Register(typeof(RouteStopsViewModel), this);
			customPresenter.Register(typeof(LicensesViewModel), this);
			customPresenter.Register(typeof(AboutViewModel), this);
			customPresenter.Register(typeof(MapViewModel), this);
			customPresenter.Register(typeof(SetAreaViewModel), this);
			customPresenter.Register(typeof(SetRouteStopViewModel), this);
			customPresenter.Register(typeof(UmbrellaRouteViewModel), this);
			customPresenter.Register(typeof(RouteViewModel), this);
			customPresenter.Register(typeof(RouteStopViewModel), this);
		}

		private Fragment FindFragment<TView>() where TView : Fragment
		{
			if (_frag2tag.ContainsKey(typeof(TView)))
			{
				return this.FragmentManager.FindFragmentByTag(_frag2tag[typeof(TView)]) as TView;
			}

			return null;
		}

		private bool NavigateBack()
		{
			if (this.FragmentManager.BackStackEntryCount == 0)
			{
				return false;
			}

			if (this.FragmentManager.BackStackEntryCount == 1)
			{
				this.ShowMap();
				return true;
			}

			this.FragmentManager.PopBackStackImmediate();
			if (this.FragmentManager.BackStackEntryCount == 1)
			{
				this.EnableDrawer();
			}

			return true;
		}

		private void Navigate(MvxViewModelRequest request, IMvxViewModelLoader loaderService)
		{
			if (request.ViewModelType == typeof(SetAreaViewModel))
			{
				var dialog = new SetAreaView();
				dialog.ViewModel = loaderService.LoadViewModel(request, null);
				dialog.Show(this.FragmentManager, null);
			}

			if (request.ViewModelType == typeof(SetRouteStopViewModel))
			{
				var dialog = new SetRouteStopView();
				dialog.ViewModel = loaderService.LoadViewModel(request, null);
				dialog.Show(this.FragmentManager, null);
			}

			if (request.ViewModelType == typeof(UmbrellaRouteViewModel))
			{
				MvxFragment umbrellaRouteView = new UmbrellaRouteView();
				umbrellaRouteView.ViewModel = loaderService.LoadViewModel(request, null);
				_frag2tag[typeof(UmbrellaRouteView)] = "umbrella_route_view";

				this.FragmentManager.BeginTransaction()
									.Replace(Resource.Id.content_frame, umbrellaRouteView, "umbrella_route_view")
									.AddToBackStack(null)
									.Commit();

				this.DisableDrawer();
			}

			if (request.ViewModelType == typeof(RouteViewModel))
			{
				MvxFragment routeView = new RouteView();
				routeView.ViewModel = loaderService.LoadViewModel(request, null);
				_frag2tag[typeof(RouteView)] = "route_view";

				this.FragmentManager.BeginTransaction()
					.Replace(Resource.Id.content_frame, routeView, "route_view")
					.AddToBackStack(null)
					.Commit();

				this.DisableDrawer();
			}

			if (request.ViewModelType == typeof(RouteStopViewModel))
			{
				MvxFragment routeStopView = new RouteStopView();
				routeStopView.ViewModel = loaderService.LoadViewModel(request, null);
				_frag2tag[typeof(RouteStopView)] = "route_stop_view";

				this.FragmentManager.BeginTransaction()
					.Replace(Resource.Id.content_frame, routeStopView, "route_stop_view")
					.AddToBackStack(null)
					.Commit();

				this.DisableDrawer();
			}
		}

		private void DisableDrawer()
		{
			_drawer.SetDrawerLockMode(DrawerLayout.LockModeLockedClosed);
			_drawerToggle.DrawerIndicatorEnabled = false;

			_drawerToggle.SyncState();
		}

		private void EnableDrawer()
		{
			_drawer.SetDrawerLockMode(DrawerLayout.LockModeUnlocked);
			_drawerToggle.DrawerIndicatorEnabled = true;
			_drawerToggle.SyncState();
		}

		private void ShowMap()
		{
			var homeViewModel = this.ViewModel as HomeViewModel;
			homeViewModel.SelectMenuItemCommand.Execute(homeViewModel.MenuItems[0]);
		}
    }
}