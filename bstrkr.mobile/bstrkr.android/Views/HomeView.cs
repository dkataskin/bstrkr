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

using Chance.MvvmCross.Plugins.UserInteraction;

using Cirrious.CrossCore;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Binding.Droid.Views;
using Cirrious.MvvmCross.Droid.FullFragging;
using Cirrious.MvvmCross.Droid.FullFragging.Fragments;
using Cirrious.MvvmCross.Droid.Views;
using Cirrious.MvvmCross.ViewModels;

using Xamarin;

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
using bstrkr.mvvm.viewmodels;
using bstrkr.mvvm.views;

namespace bstrkr.android.views
{
	[Activity(Label = "Home", 
			  LaunchMode = LaunchMode.SingleTop, 
			  Icon = "@drawable/ic_launcher")]
	public class HomeView : MvxActivity, IFragmentHost
    {
		private DrawerLayout _drawer;
		private MyActionBarDrawerToggle _drawerToggle;
		private string _drawerTitle;
		private string _title;
		private MvxListView _drawerList;
		private string _tag;
		private MenuSection _currentSection;

		public bool Show(MvxViewModelRequest request)
		{
			try
			{
				var loaderService = Mvx.Resolve<IMvxViewModelLoader>();

				if (request.ViewModelType == typeof(SetAreaViewModel))
				{
					var dialog = new SetAreaView();
					dialog.ViewModel = loaderService.LoadViewModel(request, null);
					dialog.Show(this.FragmentManager, null);

					return true;
				}

				if (request.ViewModelType == typeof(UmbrellaRouteViewModel))
				{
					MvxFragment umbrellaRouteView = new UmbrellaRouteView();
					umbrellaRouteView.ViewModel = loaderService.LoadViewModel(request, null);
					this.ActionBar.Title = (umbrellaRouteView.ViewModel as UmbrellaRouteViewModel).Title;

					this.FragmentManager.BeginTransaction()
						.Replace(Resource.Id.content_frame, umbrellaRouteView, "umbrella_route_view")
						.AddToBackStack(null)
						.Commit();

					return true;
				}

				var homeViewModel = this.ViewModel as HomeViewModel;
				MvxFragment fragment = null;
				var title = string.Empty;

				var section = homeViewModel.GetSectionForViewModelType(request.ViewModelType);

				switch (section)
				{
					case MenuSection.Map:
						title = AppResources.map_view_title;
						this.ActionBar.Title = _title = title;
						_drawerList.SetItemChecked(0, true);

						var map = this.FindFragmentById<MapView>(Resource.Id.mapView);
						if (map.ViewModel == null)
						{
							map.ViewModel = loaderService.LoadViewModel(request, null /* saved state */);;
						}

						var transaction = this.FragmentManager.BeginTransaction();
						var fragmentToRemove = this.FragmentManager.FindFragmentByTag(_tag);
						if (fragmentToRemove != null)
						{
							transaction.Remove(fragmentToRemove);
						}

						transaction.Commit();

						return true;

					case MenuSection.Routes:
						fragment = this.FindFragment<RoutesView>() ?? new RoutesView();
						title = AppResources.routes_view_title;
						_currentSection = section;
						break;

					case MenuSection.RouteStops:
						fragment = this.FindFragment<RouteStopsView>() ?? new RouteStopsView();
						title = AppResources.route_stops_view_title;
						_currentSection = section;
						break;

					case MenuSection.Preferences:
						fragment = this.FindFragment<PreferencesView>() ?? new PreferencesView();
						title = AppResources.preferences_view_title;
						_currentSection = section;
						break;

					case MenuSection.Licenses:
						fragment = this.FindFragment<LicensesView>() ?? new LicensesView();
						title = AppResources.licenses_view_title;
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

				if (fragment.Tag == _tag && _tag != null)
				{
					return true;
				}

				if (fragment.ViewModel == null)
				{
					fragment.ViewModel = loaderService.LoadViewModel(request, null /* saved state */);
				}

				_tag = Guid.NewGuid().ToString();
				this.FragmentManager.BeginTransaction()
								    .Replace(Resource.Id.content_frame, fragment, _tag)
								   	.AddToBackStack(null)
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
			var context = this.ApplicationContext;
			BusTrackerAppContext.Version = context.PackageManager
												  .GetPackageInfo(context.PackageName, 0)
												  .VersionName;

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
			customPresenter.Register(typeof(RouteStopsViewModel), this);
			customPresenter.Register(typeof(LicensesViewModel), this);
			customPresenter.Register(typeof(AboutViewModel), this);
			customPresenter.Register(typeof(MapViewModel), this);
			customPresenter.Register(typeof(SetAreaViewModel), this);
			customPresenter.Register(typeof(UmbrellaRouteViewModel), this);
		}

		private MvxFragment FindFragment<TView>() where TView : MvxFragment
		{
			return this.FragmentManager.FindFragmentById(Resource.Id.content_frame) as TView;
		}
    }
}