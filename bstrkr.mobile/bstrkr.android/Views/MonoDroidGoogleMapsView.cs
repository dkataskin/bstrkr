using System;
using System.Collections.Generic;

using System.Linq;
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
		private readonly IDictionary<string, IMapMarker> _markers = new Dictionary<string, IMapMarker>();

		private GoogleMap _map;
		private float _previousZoomValue;

		public MonoDroidGoogleMapsView(GoogleMap googleMap)
		{
			_map = googleMap;
			_map.CameraChange += this.OnCameraChange;
			_map.MarkerClick += this.OnMarkerClick;
			_map.MapClick += this.OnMapClick;

			_previousZoomValue = _map.CameraPosition.Zoom;
		}

		public event EventHandler<EventArgs> ZoomChanged;

		public event EventHandler<CameraLocationChangedEventArgs> CameraLocationChanged;

		public event EventHandler<MapMarkerClickEventArgs> MarkerClicked;

		public event EventHandler<MapClickEventArgs> MapClicked;

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

		public GeoRect VisibleRegion
		{
			get 
			{
				var visibleRegion = _map.Projection.VisibleRegion;
				return new GeoRect(
						visibleRegion.LatLngBounds.Northeast.ToGeoPoint(),
						visibleRegion.LatLngBounds.Southwest.ToGeoPoint());
			}
		}

		public void SetCamera(GeoPoint location, float zoom)
		{
			var cameraUpdate = CameraUpdateFactory.NewLatLngZoom(location.ToLatLng(), zoom);
//			_map.MoveCamera(cameraUpdate);
			_map.AnimateCamera(cameraUpdate);
		}

		public void AddMarker(IMapMarker marker)
		{
			var markerBase = marker as GoogleMapsMarkerBase;
			marker.MapView = this;

			foreach (var keyValuePair in markerBase.GetOptions())
			{
				markerBase.AttachMarker(keyValuePair.Key, _map.AddMarker(keyValuePair.Value));
			}

			foreach (var mapMarker in markerBase.Markers)
			{
				_markers[mapMarker.Value.Id] = marker;
			}
		}

		public void RemoveMarker(IMapMarker marker)
		{
			marker.MapView = null;
			var markerBase = marker as GoogleMapsMarkerBase;

			foreach (var markerKey in markerBase.Markers.Keys.ToList())
			{
				var mapId = markerBase.Markers[markerKey].Id;
				if (_markers.ContainsKey(mapId))
				{
					_markers.Remove(mapId);
				}

				markerBase.Markers[markerKey].Remove();
				markerBase.DetachMarker(markerKey);
			}
		}

		private void OnCameraChange(object sender, GoogleMap.CameraChangeEventArgs args)
		{
			if (_map.CameraPosition.Zoom != _previousZoomValue)
			{
				_previousZoomValue = _map.CameraPosition.Zoom;
				this.RaiseZoomChangedEvent();
			}

			var bounds = _map.Projection.VisibleRegion.LatLngBounds;

			this.RaiseCameraLocationChanged(
					args.Position.Target.ToGeoPoint(),
					new GeoRect(bounds.Northeast.ToGeoPoint(), bounds.Southwest.ToGeoPoint()));
		}

		private void OnMarkerClick(object sender, GoogleMap.MarkerClickEventArgs args)
		{
			if (_markers.ContainsKey(args.Marker.Id))
			{
				args.Handled = true;
				this.RaiseMapMakerClickedEvent(_markers[args.Marker.Id]);
			}
		}

		private void OnMapClick(object sender, GoogleMap.MapClickEventArgs args)
		{
			this.RaiseMapClickedEvent(args.Point.ToGeoPoint());
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

		private void RaiseMapClickedEvent(GeoPoint point)
		{
			if (this.MapClicked != null)
			{
				this.MapClicked(this, new MapClickEventArgs(point));
			}
		}

		private void RaiseCameraLocationChanged(GeoPoint location, GeoRect projectionBounds)
		{
			if (this.CameraLocationChanged != null)
			{
				this.CameraLocationChanged(this, new CameraLocationChangedEventArgs(location, projectionBounds));
			}
		}
	}
}