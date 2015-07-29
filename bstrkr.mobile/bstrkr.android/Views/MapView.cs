using System;

using Android.Gms.Common;
using Android.Gms.Maps;
using Android.OS;
using Android.Runtime;
using Android.Views;

using bstrkr.core;
using bstrkr.core.android.extensions;
using bstrkr.core.consts;
using bstrkr.core.map;
using bstrkr.core.services.location;
using bstrkr.mvvm.converters;
using bstrkr.mvvm.maps;
using bstrkr.mvvm.viewmodels;
using bstrkr.mvvm.views;

using Cirrious.CrossCore;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Droid.FullFragging.Fragments;

using Xamarin;

namespace bstrkr.android.views
{
	[Register("bstrkr.android.views.MapView")]
	public class MapView : MvxFragment, IOnMapReadyCallback
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

		public event EventHandler<EventArgs> MapClicked;

		public MapViewModel MapViewModel { get { return this.DataContext as MapViewModel; } }

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			var ignored = base.OnCreateView(inflater, container, savedInstanceState);

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
				Insights.Report(e, Xamarin.Insights.Severity.Error);
			}
		}

		public override void OnStart()
		{
			base.OnStart();
			this.InitializeMapAndHandlers();
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
			this.MapViewModel.Reload();
		}

		public override void OnPause()
		{
			base.OnPause();
			_googleMapView.OnPause();
		}

		public override void OnStop()
		{
			base.OnStop();
			if (this.MapViewModel != null)
			{
				this.MapViewModel.Stop();
			}
		}

		public override void OnLowMemory()
		{
			base.OnLowMemory();
			_googleMapView.OnLowMemory();
		}

		public void OnMapReady(GoogleMap map)
		{
			var locationProvider = Mvx.Resolve<ILocationService>();
			map.SetLocationSource(locationProvider as ILocationSource);

			if (_mapViewWrapper == null)
			{
				_mapViewWrapper = new MonoDroidGoogleMapsView(map);
				_mapViewWrapper.MarkerClicked += (s, a) =>
				{
					if (this.MapViewModel != null)
					{
						var routeStopVM = _routeStopMarkerManager.GetDataForMarker<RouteStopMapViewModel>(a.Marker);
						if (routeStopVM != null)
						{
							this.MapViewModel.ShowRouteStopInfoCommand.Execute(routeStopVM);
						} 
						else
						{
							var vehicleVM = _vehicleMarkerManager.GetDataForMarker<VehicleViewModel>(a.Marker);
							if (vehicleVM != null)
							{
								this.MapViewModel.ShowVehicleInfoCommand.Execute(vehicleVM);
							}
						}
					}
				};

				_mapViewWrapper.MapClicked += (s, a) => this.RaiseMapClickedEvent();
			}

			if (_vehicleMarkerManager == null)
			{
				_vehicleMarkerManager = new VehicleMarkerManager(_mapViewWrapper);
			}

			if (_routeStopMarkerManager == null)
			{
				_routeStopMarkerManager = new RouteStopMarkerManager(_mapViewWrapper);
			}

			if (_mapLocationManager == null)
			{
				_mapLocationManager = new MapLocationManager(_mapViewWrapper);
			}

			var set = this.CreateBindingSet<MapView, MapViewModel>();
			set.Bind(map).For(m => m.MyLocationEnabled).To(vm => vm.DetectedArea);
			set.Bind(_vehicleMarkerManager).For(m => m.ItemsSource).To(vm => vm.Vehicles);
			set.Bind(_routeStopMarkerManager).For(m => m.ItemsSource).To(vm => vm.Stops);
			set.Bind(_mapLocationManager).For(m => m.Location).To(vm => vm.Location);
			set.Bind(_mapViewWrapper).For(x => x.Zoom).To(vm => vm.Zoom);
			set.Bind(_mapViewWrapper).For("VisibleRegion").To(vm => vm.VisibleRegion);
			set.Apply();

			(this.ViewModel as MapViewModel).Zoom = map.CameraPosition.Zoom;
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

			_googleMapView.GetMapAsync(this);
		}

		private void RaiseMapClickedEvent()
		{
			if (this.MapClicked != null)
			{
				this.MapClicked(this, EventArgs.Empty);
			}
   		}
	}
}