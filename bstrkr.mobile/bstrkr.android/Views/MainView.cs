using System;

using Android.App;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.OS;

using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Droid.Views;

using bstrkr.core.android.extensions;
using bstrkr.core.android.services;
using bstrkr.core.consts;
using bstrkr.mvvm.maps;
using bstrkr.mvvm.viewmodels;
using bstrkr.mvvm.views;

namespace bstrkr.android.views
{
    [Activity(Label = "MainView")]
    public class MainView : MvxActivity
    {
		private IMapView _mapViewWrapper;
		private VehicleMarkerManager _vehicleMarkerManager;
		private MapLocationManager _mapLocationManager;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
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
															Convert.ToSingle(AppConsts.DefaultZoom));
				map.MyLocationEnabled = true;

				map.MoveCamera(cameraUpdate);

				_mapViewWrapper = new MonoDroidGoogleMapsView(map);
				_vehicleMarkerManager = new VehicleMarkerManager(_mapViewWrapper);
				_mapLocationManager = new MapLocationManager(_mapViewWrapper);
			}

			var set = this.CreateBindingSet<MainView, MainViewModel>();
			set.Bind(_vehicleMarkerManager).For(m => m.ItemsSource).To(vm => vm.Vehicles);
			set.Bind(_mapLocationManager).For(m => m.Location).To(vm => vm.Location);
			set.Apply();
		}
    }
}