using System;

using Android.App;
using Android.Gms.Common;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.OS;

using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Droid.Fragging;
using Cirrious.MvvmCross.Droid.Views;

using Xamarin;

using bstrkr.core.android.extensions;
using bstrkr.core.android.presenters;
using bstrkr.core.android.services;
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
		private IMapView _mapViewWrapper;
		private VehicleMarkerManager _vehicleMarkerManager;
		private RouteStopMarkerManager _routeStopMarkerManager;
		private MapLocationManager _mapLocationManager;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

			try 
			{
				MapsInitializer.Initialize(this.ApplicationContext);
			} 
			catch (GooglePlayServicesNotAvailableException e) 
			{
				Insights.Report(e, ReportSeverity.Error);
			}
        }

		protected override void OnViewModelSet()
		{
			base.OnViewModelSet();
			this.SetContentView(Resource.Layout.MainView);

			MapFragment mapFrag = (MapFragment)FragmentManager.FindFragmentById(Resource.Id.map);
			GoogleMap map = mapFrag.Map;
			if (map != null) 
			{
				var cameraUpdate = CameraUpdateFactory.NewLatLngZoom(
															AppConsts.DefaultLocation.ToLatLng(),
															AppConsts.DefaultZoom);
				map.MyLocationEnabled = true;

				map.MoveCamera(cameraUpdate);

				_mapViewWrapper = new MonoDroidGoogleMapsView(map);
				_vehicleMarkerManager = new VehicleMarkerManager(_mapViewWrapper);
				_routeStopMarkerManager = new RouteStopMarkerManager(_mapViewWrapper);
				_mapLocationManager = new MapLocationManager(_mapViewWrapper);
			}

			var set = this.CreateBindingSet<HomeView, HomeViewModel>();
			set.Bind(_vehicleMarkerManager).For(m => m.ItemsSource).To(vm => vm.Vehicles);
			set.Bind(_routeStopMarkerManager).For(m => m.ItemsSource).To(vm => vm.Stops);
			set.Bind(_mapLocationManager).For(m => m.Location).To(vm => vm.Location);
			set.Bind(_mapViewWrapper).For(x => x.Zoom)
									 .To(vm => vm.MarkerSize)
									 .WithConversion(new ZoomToMarkerSizeConverter());
			set.Apply();
		}
    }
}