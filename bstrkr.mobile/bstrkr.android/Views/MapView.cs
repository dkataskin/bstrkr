using System;

using Android.Gms.Common;
using Android.Gms.Maps;
using Android.OS;
using Android.Views;

using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Droid.Fragging.Fragments;

using Xamarin;

using bstrkr.core.android.extensions;
using bstrkr.core.consts;
using bstrkr.mvvm.converters;
using bstrkr.mvvm.maps;
using bstrkr.mvvm.viewmodels;
using bstrkr.mvvm.views;

namespace bstrkr.android.views
{
	public class MapView : MvxFragment
	{
		private IMapView _mapViewWrapper;
		private VehicleMarkerManager _vehicleMarkerManager;
		private RouteStopMarkerManager _routeStopMarkerManager;
		private MapLocationManager _mapLocationManager;

		public MapView()
		{
			this.RetainInstance = true;
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			var ignored = base.OnCreateView(inflater, container, savedInstanceState);
			return this.BindingInflate(Resource.Layout.fragment_map_view, null);
		}

		public override void OnViewCreated(View view, Bundle savedInstanceState)
		{
			base.OnViewCreated(view, savedInstanceState);

			try 
			{
				MapsInitializer.Initialize(this.Activity.ApplicationContext);
			} 
			catch (GooglePlayServicesNotAvailableException e) 
			{
				Insights.Report(e, ReportSeverity.Error);
			}

			var mapFragment = (MapFragment)this.FragmentManager.FindFragmentById(Resource.Id.map);
			GoogleMap map = mapFragment.Map;
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

			var set = this.CreateBindingSet<HomeView, MapViewModel>();
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