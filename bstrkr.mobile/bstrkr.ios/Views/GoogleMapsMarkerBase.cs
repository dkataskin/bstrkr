using System;

using Google.Maps;

using bstrkr.core.ios.extensions;
using bstrkr.core.spatial;
using bstrkr.mvvm.views;

namespace bstrkr.ios.views
{
	public abstract class GoogleMapsMarkerBase : Marker, IMapMarker
	{
		private IMapView _mapView;

		public GeoPoint Location
		{
			get { return this.Position.ToGeoPoint(); }
			set { this.Position = value.ToCLLocation(); }
		}

		public IMapView MapView 
		{
			get 
			{ 
				return _mapView; 
			}

			set 
			{ 
				_mapView = value;

				this.Map = value == null ? null : value.MapObject as MapView;
			}
		}
	}
}