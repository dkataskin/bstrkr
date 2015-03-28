using System;
using System.Collections.Generic;

using Android.Gms.Maps;
using Android.Gms.Maps.Model;

using bstrkr.android.views;
using bstrkr.core.android.extensions;
using bstrkr.core.spatial;
using bstrkr.mvvm.views;

namespace bstrkr.android.views
{
	public class MonoDroidGoogleMapsView : IMapView
	{
		private readonly IDictionary<Marker, IMapMarker> _markers = new Dictionary<Marker, IMapMarker>();

		private GoogleMap _map;
		private float _previousZoomValue;

		public MonoDroidGoogleMapsView(GoogleMap googleMap)
		{
			_map = googleMap;
			_map.CameraChange += this.OnCameraChange;
			_map.MarkerClick += this.OnMarkerClick;

			_previousZoomValue = _map.CameraPosition.Zoom;
		}

		public event EventHandler<EventArgs> ZoomChanged;

		public event EventHandler<EventArgs> CameraLocationChanged;

		public event EventHandler<MapMarkerClickEventArgs> MarkerClicked;

		public GeoPoint CameraLocation 
		{
			get { return _map.CameraPosition.Target.ToGeoPoint(); }
		}

		public object MapObject 
		{
			get { return _map; }
		}

		public float Zoom 
		{
			get { return _map.CameraPosition.Zoom; }
		}

		public void SetCamera(GeoPoint location, float zoom)
		{
			var cameraUpdate = CameraUpdateFactory.NewLatLngZoom(location.ToLatLng(), zoom);
			_map.MoveCamera(cameraUpdate);
		}

		public void AddMarker(IMapMarker marker)
		{
			var markerBase = marker as GoogleMapsMarkerBase;
			marker.MapView = this;

			markerBase.Marker = _map.AddMarker(markerBase.GetOptions());
			_markers[markerBase.Marker] = marker;
		}

		public void RemoveMarker(IMapMarker marker)
		{
			marker.MapView = null;
			var markerBase = marker as GoogleMapsMarkerBase;
			if (_markers.ContainsKey(markerBase.Marker))
			{
				_markers.Remove(markerBase.Marker);
			}

			markerBase.Marker.Remove();
		}

		private void OnCameraChange(object sender, GoogleMap.CameraChangeEventArgs args)
		{
			if (_map.CameraPosition.Zoom != _previousZoomValue)
			{
				_previousZoomValue = _map.CameraPosition.Zoom;
				this.RaiseZoomChangedEvent();
			}
		}

		private void OnMarkerClick(object sender, GoogleMap.MarkerClickEventArgs args)
		{
			args.Handled = true;
			if (_markers.Contains(args.Marker))
			{
				this.RaiseMapMakerClickedEvent(_markers[args.Marker]);
			}
		}

		private void RaiseZoomChangedEvent()
		{
			if (this.ZoomChanged != null)
			{
				this.ZoomChanged(this, EventArgs.Empty);
			}
		}

		private void RaiseMapMakerClickedEvent(IMapMarker marker)
		{
			if (this.MarkerClicked != null)
			{
				this.MarkerClicked(this, new MapMarkerClickEventArgs(marker));
			}
		}
	}
}