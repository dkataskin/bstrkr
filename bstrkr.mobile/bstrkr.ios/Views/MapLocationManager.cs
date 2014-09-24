using System;

using Google.Maps;

using MonoTouch.CoreLocation;

using bstrkr.core.spatial;

namespace bstrkr.ios.views
{
	public class MapLocationManager
	{
		private readonly MapView _mapView;
		private GeoPoint _location;

		public MapLocationManager(MapView mapView)
		{
			_mapView = mapView;
		}

		public virtual GeoPoint Location
		{
			get 
			{ 
				return _location; 
			}

			set
			{
				this.SetLocation(value);
			}
		}

		protected virtual void SetLocation(GeoPoint location)
		{
			_location = location;

			_mapView.Camera = CameraPosition.FromCamera(
								new CLLocationCoordinate2D(location.Latitude, location.Longitude),
								_mapView.Camera.Zoom);
		}
	}
}