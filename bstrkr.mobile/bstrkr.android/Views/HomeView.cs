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
using Android.Widget;

using Chance.MvvmCross.Plugins.UserInteraction;

using Cirrious.CrossCore;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Binding.Droid.Views;
using Cirrious.MvvmCross.Droid.Fragging;
using Cirrious.MvvmCross.Droid.Fragging.Fragments;
using Cirrious.MvvmCross.Droid.Views;
using Cirrious.MvvmCross.ViewModels;

using Xamarin;

using bstrkr.core;
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
		private DrawerLayout _drawer;
		private MyActionBarDrawerToggle _drawerToggle;
		private string _drawerTitle;
		private string _title;
		private MvxListView _drawerList;
		private string _tag;

		public bool Show(MvxViewModelRequest request)
		{
			try
			{
				if (request.ViewModelType == typeof(SetAreaViewModel))
				{
					var loaderService = Mvx.Resolve<IMvxViewModelLoader>();
					var viewModel = loaderService.LoadViewModel(request, null);
					var dialog = new SetAreaView();
					dialog.ViewModel = viewModel;

					dialog.Show(this.SupportFragmentManager, null);

					return true;
				}

				var homeViewModel = this.ViewModel as HomeViewModel;
				MvxFragment fragment = null;
				var title = string.Empty;

				var section = homeViewModel.GetSectionForViewModelType(request.ViewModelType);

				var frame = this.FindViewById<FrameLayout>(Resource.Id.content_frame);
				switch (section)
				{
					case MenuSection.Map:
						title = Resources.GetString(Resource.String.map_view_title);
						this.ActionBar.Title = _title = title;

						var map = this.FindFragmentById<MapView>(Resource.Id.mapView);
						if (map.ViewModel == null)
						{
							var loaderService1 = Mvx.Resolve<IMvxViewModelLoader>();
							var viewModel1 = loaderService1.LoadViewModel(request, null /* saved state */);
							map.ViewModel = viewModel1;
						}

						var tr = this.SupportFragmentManager.BeginTransaction();
						var fragmentToRemove = this.SupportFragmentManager.FindFragmentByTag(_tag);
						if (fragmentToRemove != null)
						{
							tr.Remove(fragmentToRemove);
						}

						tr.Commit();

						return true;
						break;

					case MenuSection.Routes:
						fragment = this.FindFragment<RoutesView>();

						if (fragment == null)
						{
							fragment = new RoutesView();
						}

						title = Resources.GetString(Resource.String.routes_view_title);
						break;

					case MenuSection.Preferences:
						fragment = this.FindFragment<PreferencesView>();

						if (fragment == null)
						{
							fragment = new PreferencesView();
						}

						title = Resources.GetString(Resource.String.prefs_view_title);
						break;

					case MenuSection.Licenses:
						fragment = this.FindFragment<LicensesView>();
						if (fragment == null)
						{
							fragment = new LicensesView();
						}

						title = Resources.GetString(Resource.String.licenses_view_title);
						break;

					case MenuSection.About:
						var position = _drawerList.SelectedItemPosition;
						Mvx.Resolve<IUserInteraction>().Alert(
												AppResources.about_view_text,
												() => _drawerList.SetItemChecked(position, true),
												AppResources.about_view_title,
												AppResources.ok);

						return true;
						break;
				}

				if (fragment.Tag == _tag && _tag != null)
				{
					return true;
				}

				if (fragment.ViewModel == null)
				{
					var loaderService = Mvx.Resolve<IMvxViewModelLoader>();
					var viewModel = loaderService.LoadViewModel(request, null /* saved state */);

					fragment.ViewModel = viewModel;
				}

				_tag = Guid.NewGuid().ToString();
				this.SupportFragmentManager.BeginTransaction()
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
			customPresenter.Register(typeof(LicensesViewModel), this);
			customPresenter.Register(typeof(AboutViewModel), this);
			customPresenter.Register(typeof(MapViewModel), this);
			customPresenter.Register(typeof(SetAreaViewModel), this);
		}

		private MvxFragment FindFragment<TView>() where TView : MvxFragment
		{
			return this.SupportFragmentManager.FindFragmentById(Resource.Id.content_frame) as TView;
		}
    }
}