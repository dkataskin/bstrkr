using System;

using bstrkr.core.spatial;
using bstrkr.mvvm.views;

namespace bstrkr.mvvm.maps
{
	public class MapLocationManager
	{
		private readonly IMapView _mapView;
		private GeoPoint _location;

		public MapLocationManager(IMapView mapView)
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

			_mapView.SetCamera(location.Latitude, location.Longitude, _mapView.Zoom);
		}
	}
}