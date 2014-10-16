using System;

using Google.Maps;

using MonoTouch.CoreLocation;
using MonoTouch.Foundation;

using bstrkr.core.ios.extensions;
using bstrkr.core.spatial;
using bstrkr.mvvm.views;

namespace bstrkr.ios.views
{
	public class MonoTouchGoogleMapsView : IMapView
	{
		private readonly MapView _mapView;
		private float _previousZoomValue;

		public MonoTouchGoogleMapsView(MapView mapView)
		{
			_mapView = mapView;
			_mapView.CameraPositionChanged += this.OnCameraPositionChanged;
			_previousZoomValue = _mapView.Camera.Zoom;
		}

		public event EventHandler<EventArgs> ZoomChanged;

		public object MapObject
		{
			get { return _mapView; }
		}

		public float Zoom 
		{
			get { return _mapView.Camera.Zoom; }
		}

		public void SetCamera(GeoPoint location, float zoom)
		{
			_mapView.Camera = CameraPosition.FromCamera(location.ToCLLocation(), zoom);
		}

		public void AddMarker(IMarker marker)
		{
			(marker as Marker).Map = _mapView;
		}

		public void RemoveMarker(IMarker marker)
		{
			(marker as Marker).Map = null;
		}

		private void OnCameraPositionChanged(object sender, GMSCameraEventArgs args)
		{
			if (_previousZoomValue != _mapView.Camera.Zoom)
			{
				_previousZoomValue = _mapView.Camera.Zoom;
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