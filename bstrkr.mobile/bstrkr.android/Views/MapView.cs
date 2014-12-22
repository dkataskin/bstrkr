using System;

using Android.Gms.Common;
using Android.Gms.Maps;
using Android.OS;
using Android.Views;

using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Droid.FullFragging.Fragments;

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
		private Android.Gms.Maps.MapView _googleMapView;

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

			//this.EnsureBindingContextIsSet(savedInstanceState);
			var view = this.BindingInflate(Resource.Layout.fragment_map_view, null);
			_googleMapView = view.FindViewById<Android.Gms.Maps.MapView>(Resource.Id.mapView);
			_googleMapView.OnCreate(savedInstanceState);

			return view;
		}

		public override void OnActivityCreated(Bundle savedInstanceState)
		{
			base.OnActivityCreated(savedInstanceState);

			try 
			{
				MapsInitializer.Initialize(this.Activity);
			} 
			catch (GooglePlayServicesNotAvailableException e) 
			{
				Insights.Report(e, ReportSeverity.Error);
			}
		}

		public override void OnStart()
		{
			base.OnStart();
			InitializeMapAndHandlers();
		}

		public override void OnDestroyView()
		{
			base.OnDestroyView();
			_googleMapView.OnDestroy();
		}

		public override void OnResume()
		{
			base.OnResume();
			SetUpMapIfNeeded();
			_googleMapView.OnResume();
		}

		public override void OnPause()
		{
			base.OnPause();
			_googleMapView.OnPause();
		}

		public override void OnLowMemory()
		{
			base.OnLowMemory();
			_googleMapView.OnLowMemory();
		}

		private void SetUpMapIfNeeded()
		{
			if (_googleMapView == null)
			{
				_googleMapView = View.FindViewById<Android.Gms.Maps.MapView>(Resource.Id.mapView);
			}
		}

		private void InitializeMapAndHandlers()
		{
			this.SetUpMapIfNeeded();

			GoogleMap map = _googleMapView.Map;
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

			var set = this.CreateBindingSet<MapView, MapViewModel>();
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