using System;

using Google.Maps;

using MonoTouch.CoreLocation;

using bstrkr.mvvm.views;
using System.Diagnostics;

namespace bstrkr.ios.views
{
	public class MonoTouchGoogleMapsView : IMapView
	{
		private readonly MapView _mapView;

		public MonoTouchGoogleMapsView(MapView mapView)
		{
			_mapView = mapView;
			_mapView.CameraPositionChanged += this.OnCameraPositionChanged;
		}

		public object MapObject
		{
			get { return _mapView; }
		}

		public double Zoom 
		{
			get { return _mapView.Camera.Zoom; }
		}

		public void SetCamera(double latitude, double longitude, double zoom)
		{
			_mapView.Camera = CameraPosition.FromCamera(
									new CLLocationCoordinate2D(latitude, longitude),
									Convert.ToSingle(zoom));
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
			Debug.WriteLine("position changed, Zoom={0:F2}", _mapView.Camera.Zoom);
		}
	}
}