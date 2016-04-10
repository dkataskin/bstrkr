using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Gms.Common;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Views;
using Android.Widget;

using bstrkr.android.services.resources;
using bstrkr.android.util;
using bstrkr.core;
using bstrkr.core.android;
using bstrkr.core.android.extensions;
using bstrkr.core.android.presenters;
using bstrkr.core.android.services;
using bstrkr.core.android.views;
using bstrkr.core.consts;
using bstrkr.core.context;
using bstrkr.core.services.location;
using bstrkr.core.services.resources;
using bstrkr.core.utils;
using bstrkr.mvvm.converters;
using bstrkr.mvvm.maps;
using bstrkr.mvvm.messages;
using bstrkr.mvvm.viewmodels;
using bstrkr.mvvm.views;

using Chance.MvvmCross.Plugins.UserInteraction;

using Cheesebaron.SlidingUpPanel;

using Cirrious.CrossCore;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Binding.Droid.Views;
using Cirrious.MvvmCross.Droid.FullFragging;
using Cirrious.MvvmCross.Droid.FullFragging.Fragments;
using Cirrious.MvvmCross.Droid.Views;
using Cirrious.MvvmCross.Plugins.Messenger;
using Cirrious.MvvmCross.ViewModels;

using SmoothProgressBarSharp;

namespace bstrkr.android.views
{
	[Activity(Label = "Home", 
			  LaunchMode = LaunchMode.SingleTop, 
			  Icon = "@drawable/ic_launcher",
			  ScreenOrientation = ScreenOrientation.Portrait,
			  Theme = "@style/BusTrackerTheme")]
	[Register("bstrkr.android.views.HomeView")]
	public class HomeView : MvxAppCompatActivity, IFragmentHost, IMenuItemOnMenuItemClickListener
    {
		private DrawerLayout _drawer;
		private BusTrackerActionBarDrawerToggle _drawerToggle;
		private NavigationView _navigationView;
		private MenuSection _currentSection;

		private bool _enableDrawerOnNextNavigation;

		private Fragment _slidingPanelFragment;

		private static IDictionary<Type, string> _frag2tag = new Dictionary<Type, string>();

		protected HomeViewModel HomeViewModel { get { return this.ViewModel as HomeViewModel; } }

		public bool Show(MvxViewModelRequest request)
		{
			try
			{
				if (_enableDrawerOnNextNavigation)
				{
					this.EnableDrawer();
					_enableDrawerOnNextNavigation = false;
				}

				var loaderService = Mvx.Resolve<IMvxViewModelLoader>();
				var homeViewModel = this.ViewModel as HomeViewModel;
				MvxFragment fragment = null;

				var section = homeViewModel.GetSectionForViewModelType(request.ViewModelType);
				if (section == MenuSection.Unknown)
				{
					this.Navigate(request, loaderService);
					return true;
				}

				this.CloseSlidingPanel();

				switch (section)
				{
					case MenuSection.Map:
						_currentSection = MenuSection.Map;

						this.SupportActionBar.Title = homeViewModel.Title;

						_navigationView.SetCheckedItem(0);

						var map = this.FragmentManager.FindFragmentById(Resource.Id.mapView) as MapView;
						if (map.ViewModel == null)
						{
							map.MapClicked += (s, a) => this.CloseSlidingPanel();
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

						this.CloseSlidingPanel();

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
						fragment = this.FindFragment<AboutView>() ?? new AboutView();
						_currentSection = section;
						break;
				}

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
				_navigationView.SetCheckedItem(homeViewModel.MenuItems.IndexOf(menuItem));

				_drawer.CloseDrawer(_navigationView);

				return true;
			}
			finally
			{
				_drawer.CloseDrawer(_navigationView); 
			}
		}

		public override bool OnCreateOptionsMenu(IMenu menu)
		{
			var menuInflater = this.MenuInflater;
			menuInflater.Inflate(Resource.Menu.main_menu, menu);

			var refreshMenuItem = menu.FindItem(Resource.Id.menu_refresh);
			refreshMenuItem.SetOnMenuItemClickListener(this);

			return true;
		}

		public bool OnMenuItemClick(IMenuItem item)
		{
			if (item.ItemId == Resource.Id.menu_refresh)
			{
				(this.ViewModel as HomeViewModel).UpdateVehicleLocationsCommand.Execute();
				return true;
			}

			return false;
		}

		protected override void OnPostCreate(Bundle savedInstanceState)
		{
			base.OnPostCreate(savedInstanceState);
			_drawerToggle.SyncState();

			this.SetUpSlidingPanel();
			this.SetUpCitiesSpinner();
			this.SetUpIconGenerator();
			this.CloseSlidingPanel();
		}

		private void SetUpSlidingPanel()
		{
			var panel = this.FindViewById<SlidingUpPanelLayout>(Resource.Id.sliding_layout);
			panel.AnchorPoint = 0.4f;
			panel.CoveredFadeColor = new Android.Graphics.Color(0x00FFFFFF);
			panel.SlidingEnabled = true;
		}

		private void SetUpCitiesSpinner()
		{
			var headerView = _navigationView.GetHeaderView(0);
			var spinner = headerView.FindViewById<Spinner>(Resource.Id.city);
//			spinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(spinner_ItemSelected);
			var adapter = new ArrayAdapter<AreaViewModel>(
												this.BaseContext,
//												Resource.Layout.item_area_spinner,
												Resource.Layout.support_simple_spinner_dropdown_item,
												this.HomeViewModel.Areas.ToArray());

			adapter.SetDropDownViewResource(Resource.Layout.support_simple_spinner_dropdown_item);
			spinner.Adapter = adapter;
		}

		private void SetUpIconGenerator()
		{
			var appResourcesManager = Mvx.Resolve<IAppResourceManager>() as AndroidAppResourceManager;
			appResourcesManager.IconGenerator = new IconGenerator(this.BaseContext);
		}

		public override void OnConfigurationChanged(Configuration newConfig)
		{
			base.OnConfigurationChanged(newConfig);
			_drawerToggle.OnConfigurationChanged(newConfig);
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

		public override void OnBackPressed()
		{
			if (!this.NavigateBack())
			{
				base.OnBackPressed();
			}
		}

		protected override void OnSaveInstanceState(Bundle outState)
		{
		}

		protected override void OnRestoreInstanceState(Bundle savedInstanceState)
		{
		}

		public override void OnRestoreInstanceState(Bundle savedInstanceState, PersistableBundle persistentState)
		{
		}

        protected override void OnCreate(Bundle savedInstanceState)
        {
			var context = this.ApplicationContext;
			BusTrackerAppContext.Version = context.PackageManager
												  .GetPackageInfo(context.PackageName, 0)
												  .VersionName;

            base.OnCreate(savedInstanceState);

			this.SetContentView(Resource.Layout.page_home_view);

			var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
			this.SetSupportActionBar(toolbar);

			_drawer = this.FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
			_navigationView = this.FindViewById<NavigationView>(Resource.Id.nav_view);
			_navigationView.NavigationItemSelected += this.OnNavigationItemSelected;
			_drawer.SetDrawerShadow(Resource.Drawable.drawer_shadow_dark, (int)GravityFlags.Start);

			//DrawerToggle is the animation that happens with the indicator next to the
			//ActionBar icon.
			_drawerToggle = new BusTrackerActionBarDrawerToggle(
													this,
													_drawer,
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

			this.EnableDrawer();

			this.SupportActionBar.SetDisplayHomeAsUpEnabled(true);
			this.SupportActionBar.SetHomeButtonEnabled(true);
			this.SupportActionBar.Title = AppResources.map_view_title;

			var progressView = this.FindViewById<SmoothProgressBar>(Resource.Id.progressbar);
			var set = this.CreateBindingSet<HomeView, HomeViewModel>();
			set.Bind(progressView)
				.For(lt => lt.Visibility)
				.To(vm => vm.IsBusy)
				.WithConversion("Visibility");

			set.Apply();

			this.ShowMap();
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
			customPresenter.Register(typeof(SetRouteStopViewModel), this);
			customPresenter.Register(typeof(SetRouteViewModel), this);
			customPresenter.Register(typeof(RouteVehiclesViewModel), this);
			customPresenter.Register(typeof(RouteStopViewModel), this);
			customPresenter.Register(typeof(VehicleForecastViewModel), this);
		}

		private MvxFragment FindFragment<TView>() where TView : MvxFragment
		{
			if (_frag2tag.ContainsKey(typeof(TView)))
			{
				return this.FragmentManager.FindFragmentByTag(_frag2tag[typeof(TView)]) as TView;
			}

			return null;
		}

		private bool NavigateBack()
		{
			if (this.CloseSlidingPanel())
			{
				var map = this.FragmentManager.FindFragmentById(Resource.Id.mapView) as MapView;
				map.ClearSelection();

				return true;
			}

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
			if (request.ViewModelType == typeof(SetRouteStopViewModel))
			{
				var dialog = new SetRouteStopView();
				dialog.ViewModel = loaderService.LoadViewModel(request, null);
				dialog.Show(this.FragmentManager, null);
			}

			if (request.ViewModelType == typeof(SetRouteViewModel))
			{
				var dialog = new SetRouteView();
				dialog.ViewModel = loaderService.LoadViewModel(request, null);
				dialog.Show(this.FragmentManager, null);
			}

			if (request.ViewModelType == typeof(RouteVehiclesViewModel))
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

			if (request.ViewModelType == typeof(VehicleForecastViewModel))
			{
				if (request.RequestedBy != null)
				{
					if (request.RequestedBy.AdditionalInfo == "map_tap")
					{
						var vehicleForecastView = new RouteVehicleForecastView();
						vehicleForecastView.ViewModel = loaderService.LoadViewModel(request, null);

						this.ShowMap();

						this.ShowInSlidingPanel(vehicleForecastView);

						this.EnableDrawer();
					}
				}
			}

			if (request.ViewModelType == typeof(RouteStopViewModel))
			{
				if (request.RequestedBy != null)
				{
					if (request.RequestedBy.AdditionalInfo == "map_tap")
					{
						var routeStopView1 = new MapRouteStopView();
						routeStopView1.ViewModel = loaderService.LoadViewModel(request, null);

						this.ShowMap();

						this.ShowInSlidingPanel(routeStopView1);

						this.EnableDrawer();
					}
				}
				else
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
			_navigationView.SetCheckedItem(homeViewModel.MenuItems.IndexOf(homeViewModel.MenuItems.First(x => x.Section == MenuSection.Map)));
			homeViewModel.SelectMenuItemCommand.Execute(MenuSection.Map);
		}

		private bool CloseSlidingPanel()
		{
			var panel = this.FindViewById<SlidingUpPanelLayout>(Resource.Id.sliding_layout);
			if (panel.PaneVisible)
			{
				if (_slidingPanelFragment != null)
				{
					this.FragmentManager.BeginTransaction()
											   .Remove(_slidingPanelFragment)
											   .Commit();
				}

				_slidingPanelFragment = null;
				panel.HidePane();

				return true;
			}

			return false;
		}

		private void ShowInSlidingPanel(Fragment fragment)
		{
			if (_currentSection != MenuSection.Map)
			{
				return;
			}

			var panelFrame = this.FindViewById<FrameLayout>(Resource.Id.panel_content_frame);
			if (panelFrame.Visibility != ViewStates.Visible)
			{
				panelFrame.Visibility = ViewStates.Visible;
			}

			var panel = this.FindViewById<SlidingUpPanelLayout>(Resource.Id.sliding_layout);
			if (!panel.PaneVisible)
			{
				panel.ShowPane();
			}
				
			var transaction = this.FragmentManager.BeginTransaction();
			if (_slidingPanelFragment != null)
			{
				transaction.Remove(_slidingPanelFragment);
			}

			transaction.Add(Resource.Id.panel_content_frame, fragment)
					   .Commit();

			_slidingPanelFragment = fragment;

			if (panel.IsExpanded)
			{
				panel.CollapsePane();
			}
		}

		private void OnNavigationItemSelected(object sender, NavigationView.NavigationItemSelectedEventArgs args)
		{
			var homeViewModel = this.ViewModel as HomeViewModel;
			switch (args.MenuItem.ItemId) 
			{
				case Android.Resource.Id.Home:
					_drawer.OpenDrawer(Android.Support.V4.View.GravityCompat.Start);
					break;

				case Resource.Id.nav_map:
					homeViewModel.SelectMenuItemCommand.Execute(MenuSection.Map);
					break;

				case Resource.Id.nav_routes:
					homeViewModel.SelectMenuItemCommand.Execute(MenuSection.Routes);
					break;

				case Resource.Id.nav_stops:
					homeViewModel.SelectMenuItemCommand.Execute(MenuSection.RouteStops);
					break;

				case Resource.Id.nav_preferences:
					homeViewModel.SelectMenuItemCommand.Execute(MenuSection.Preferences);
					break;

				case Resource.Id.nav_about:
					homeViewModel.SelectMenuItemCommand.Execute(MenuSection.About);
					break;
			}

			base.OnOptionsItemSelected(args.MenuItem);
		}
    }
}