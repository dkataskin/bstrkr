﻿using System;

using Android.Gms.Maps;
using Android.Gms.Maps.Model;

using bstrkr.core.android.extensions;
using bstrkr.core.spatial;
using bstrkr.mvvm.views;
using bstrkr.android.views;

namespace bstrkr.android.views
{
	public class MonoDroidGoogleMapsView : IMapView
	{
		private GoogleMap _map;
		private float _previousZoomValue;

		public MonoDroidGoogleMapsView(GoogleMap googleMap)
		{
			_map = googleMap;
			_map.CameraChange += this.OnCameraChange;

			_previousZoomValue = _map.CameraPosition.Zoom;
		}

		public event EventHandler<EventArgs> ZoomChanged;

		public void SetCamera(GeoPoint location, float zoom)
		{
			var cameraUpdate = CameraUpdateFactory.NewLatLngZoom(location.ToLatLng(), zoom);
			_map.MoveCamera(cameraUpdate);
		}

		public object MapObject 
		{
			get { return _map; }
		}

		public float Zoom 
		{
			get { return _map.CameraPosition.Zoom; }
		}

		public void AddMarker(IMarker marker)
		{
			var markerBase = marker as GoogleMapsMarkerBase;
			marker.MapView = this;

			markerBase.Marker = _map.AddMarker(markerBase.GetOptions());
		}

		public void RemoveMarker(IMarker marker)
		{
			marker.MapView = null;
			var markerBase = marker as GoogleMapsMarkerBase;
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

		private void RaiseZoomChangedEvent()
		{
			if (this.ZoomChanged != null)
			{
				this.ZoomChanged(this, EventArgs.Empty);
			}
		}
	}
}