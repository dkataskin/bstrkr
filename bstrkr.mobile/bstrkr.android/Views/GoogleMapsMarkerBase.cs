using System;

using Android.Gms.Maps.Model;

using bstrkr.core.android.extensions;
using bstrkr.core.spatial;
using bstrkr.mvvm.views;

namespace bstrkr.android.views
{
	public abstract class GoogleMapsMarkerBase : IMapMarker
	{
		public Marker Marker { get; set; }

		public IMapView MapView { get; set; }

		public GeoPoint Location 
		{
			get 
			{
				return this.Marker == null ? GeoPoint.Empty : this.Marker.Position.ToGeoPoint();
			}

			set
			{
				if (this.Marker != null)
				{
					this.Marker.Position = value.ToLatLng();
				}
			}
		}

		public abstract MarkerOptions GetOptions();
	}
}