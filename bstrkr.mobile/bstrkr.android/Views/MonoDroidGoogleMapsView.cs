using System;

using Android.Gms.Maps;
using Android.Gms.Maps.Model;

using bstrkr.mvvm.views;

namespace Views
{
	public class MonoDroidGoogleMapsView : IMapView
	{
		private GoogleMap _map;

		public MonoDroidGoogleMapsView(GoogleMap googleMap)
		{
			_map = googleMap;
		}

		public void SetCamera(double latitude, double longitude, double zoom)
		{
			var cameraUpdate = CameraUpdateFactory.NewLatLngZoom(latitude, longitude, Convert.ToSingle(zoom));
			_map.MoveCamera(cameraUpdate);
		}

		public object MapObject 
		{
			get { return _map; }
		}

		public double Zoom 
		{
			get { return Convert.ToDouble(_map.CameraPosition.Zoom); }
		}

		public void AddMarker(IMarker marker)
		{
		}

		public void RemoveMarker(IMarker marker)
		{
		}
	}
}