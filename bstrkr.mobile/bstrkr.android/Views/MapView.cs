using System;

using Android.Gms.Common;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
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
using bstrkr.core.spatial;

namespace bstrkr.android.views
{
	[Register("bstrkr.android.views.MapView")]
	public class MapView : MvxFragment, IOnMapReadyCallback
	{
		private MapFragment _mapFragment;
		private IMapView _mapViewWrapper;
		private VehicleMarkerManager _vehicleMarkerManager;
		private RouteStopMarkerManager _routeStopMarkerManager;
		private MapLocationManager _mapLocationManager;
		private Marker _myLocationMarker;

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

			var mapOptions = new GoogleMapOptions()
				.InvokeTiltGesturesEnabled(false)
				.InvokeCompassEnabled(true)
				.InvokeRotateGesturesEnabled(true)
				.InvokeUseViewLifecycleInFragment(false)
				.InvokeZoomControlsEnabled(true)
				.InvokeZoomGesturesEnabled(true)
				.InvokeZOrderOnTop(true)
				.InvokeCamera(new CameraPosition(new LatLng(54.180109f, 45.177947f), 14.0f, 0, 0));

			_mapFragment = MapFragment.NewInstance(mapOptions);
			var transaction = this.FragmentManager.BeginTransaction();
			transaction.Add(Resource.Id.content_frame, _mapFragment);
			transaction.Commit();

			_mapFragment.GetMapAsync(this);

			return view;
		}

		public override void OnResume()
		{
			base.OnResume();
			this.MapViewModel.Reload();
		}

		public override void OnStop()
		{
			base.OnStop();
			if (this.MapViewModel != null)
			{
				this.MapViewModel.Stop();
			}
		}

		public void OnMapViewportChanged(float visibleRegionOffset)
		{
			var mapViewModel = this.ViewModel as MapViewModel;
			if (mapViewModel != null)
			{
				mapViewModel.ChangeMapViewportCommand.Execute(visibleRegionOffset);
			}
		}

		public void ClearSelection()
		{
			var mapViewModel = this.ViewModel as MapViewModel;
			if (mapViewModel != null)
			{
				mapViewModel.ClearSelectionCommand.Execute();
			}
		}

		public void OnMapReady(GoogleMap map)
		{
			map.MyLocationButtonClick += (s, a) =>
			{
				if (_myLocationMarker != null)
				{
					var point = _myLocationMarker.Position.ToGeoPoint();
					this.MapViewModel.UpdateMapCenterCommand.Execute(new Tuple<GeoPoint, bool>(point, true));
				}
			};
				
			var locationProvider = Mvx.Resolve<ILocationService>();

			var lastLocation = locationProvider.GetLastLocation();
			if (!lastLocation.Equals(GeoPoint.Empty))
			{
				var myLocationMarkerOptions = new MarkerOptions();
				myLocationMarkerOptions.SetIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.my_location));
				myLocationMarkerOptions.Flat(true);
				myLocationMarkerOptions.SetPosition(lastLocation.ToLatLng());
				_myLocationMarker = map.AddMarker(myLocationMarkerOptions);
			}

			locationProvider.LocationUpdated += this.OnLocationUpdated;
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
							this.MapViewModel.SelectRouteStopCommand.Execute(routeStopVM.Model.Id);
						} 
						else
						{
							var vehicleVM = _vehicleMarkerManager.GetDataForMarker<VehicleViewModel>(a.Marker);
							if (vehicleVM != null)
							{
								this.MapViewModel.SelectVehicleCommand.Execute(vehicleVM.VehicleId);
							}
						}
					}
				};

				_mapViewWrapper.MapClicked += (s, a) =>
				{
					this.RaiseMapClickedEvent();
					this.MapViewModel.ClearSelectionCommand.Execute();
				};

				_mapViewWrapper.CameraLocationChanged += (s, a) => this.MapViewModel.UpdateMapCenterCommand.Execute(new Tuple<GeoPoint, bool>(a.Location, false));
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
			set.Bind(map)
			   .For(m => m.MyLocationEnabled)
			   .To(vm => vm.DetectedArea);

			set.Bind(_vehicleMarkerManager)
			   .For(m => m.ItemsSource)
			   .To(vm => vm.Vehicles);
			
			set.Bind(_routeStopMarkerManager)
			   .For(m => m.ItemsSource)
			   .To(vm => vm.Stops);
			
			set.Bind(_mapLocationManager)
			   .For(m => m.Location)
			   .To(vm => vm.MapCenter);
			
			set.Bind(_mapViewWrapper)
			   .For(m => m.Zoom)
			   .To(vm => vm.Zoom);
			
			set.Bind(_mapViewWrapper)
			   .For(m => m.VisibleRegion)
			   .To(vm => vm.VisibleRegion);
			
			set.Apply();

			(this.ViewModel as MapViewModel).Zoom = map.CameraPosition.Zoom;
		}

		private void RaiseMapClickedEvent()
		{
			if (this.MapClicked != null)
			{
				this.MapClicked(this, EventArgs.Empty);
			}
   		}

		private void OnLocationUpdated(object source, LocationUpdatedEventArgs args)
		{
			var marker = _myLocationMarker;
			if (marker != null)
			{
				marker.Position = args.Location.ToLatLng();
			}
		}
	}
}