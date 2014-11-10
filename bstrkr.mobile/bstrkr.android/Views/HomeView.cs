using System;

using Android.App;
using Android.Gms.Common;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Views;

using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Binding.Droid.Views;
using Cirrious.MvvmCross.Droid.Fragging;
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

//		private IMapView _mapViewWrapper;
//		private VehicleMarkerManager _vehicleMarkerManager;
//		private RouteStopMarkerManager _routeStopMarkerManager;
//		private MapLocationManager _mapLocationManager;

		public bool Show(MvxViewModelRequest request)
		{
			throw new NotImplementedException();
		}

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
			this.SetContentView(Resource.Layout.page_home_view);

			_title = _drawerTitle = this.Title;
			_drawer = this.FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
			_drawerList = this.FindViewById<MvxListView>(Resource.Id.left_drawer);

			//_drawer.SetDrawerShadow(Resource.Drawable.drawer_shadow_dark, (int)GravityFlags.Start);

			this.ActionBar.SetDisplayHomeAsUpEnabled(true);
			this.ActionBar.SetHomeButtonEnabled(true);

			//DrawerToggle is the animation that happens with the indicator next to the
			//ActionBar icon.
//			this._drawerToggle = new MyActionBarDrawerToggle(this, this._drawer,
//				Resource.Drawable.ic_drawer_light,
//				Resource.String.drawer_open,
//				Resource.String.drawer_close);

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


//			this.RegisterForDetailsRequests();
//
//			if (null == savedInstanceState)
//			{
//				this.ViewModel.SelectMenuItemCommand.Execute(this.ViewModel.MenuItems[0]);
//			}

//			try 
//			{
//				MapsInitializer.Initialize(this.ApplicationContext);
//			} 
//			catch (GooglePlayServicesNotAvailableException e) 
//			{
//				Insights.Report(e, ReportSeverity.Error);
//			}


        }

		protected override void OnViewModelSet()
		{
			base.OnViewModelSet();


//			MapFragment mapFrag = (MapFragment)FragmentManager.FindFragmentById(Resource.Id.map);
//			GoogleMap map = mapFrag.Map;
//			if (map != null) 
//			{
//				var cameraUpdate = CameraUpdateFactory.NewLatLngZoom(
//															AppConsts.DefaultLocation.ToLatLng(),
//															AppConsts.DefaultZoom);
//				map.MyLocationEnabled = true;
//
//				map.MoveCamera(cameraUpdate);
//
//				_mapViewWrapper = new MonoDroidGoogleMapsView(map);
//				_vehicleMarkerManager = new VehicleMarkerManager(_mapViewWrapper);
//				_routeStopMarkerManager = new RouteStopMarkerManager(_mapViewWrapper);
//				_mapLocationManager = new MapLocationManager(_mapViewWrapper);
//			}
//
//			var set = this.CreateBindingSet<HomeView, HomeViewModel>();
//			set.Bind(_vehicleMarkerManager).For(m => m.ItemsSource).To(vm => vm.Vehicles);
//			set.Bind(_routeStopMarkerManager).For(m => m.ItemsSource).To(vm => vm.Stops);
//			set.Bind(_mapLocationManager).For(m => m.Location).To(vm => vm.Location);
//			set.Bind(_mapViewWrapper).For(x => x.Zoom)
//									 .To(vm => vm.MarkerSize)
//									 .WithConversion(new ZoomToMarkerSizeConverter());
//			set.Apply();
		}
    }
}