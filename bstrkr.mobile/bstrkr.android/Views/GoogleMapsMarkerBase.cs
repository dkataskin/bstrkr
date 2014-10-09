using System;

using Android.Gms.Maps.Model;

using bstrkr.mvvm.views;

namespace bstrkr.android.views
{
	public abstract class GoogleMapsMarkerBase : IMarker
	{
		public Marker Marker { get; set; }

		public IMapView MapView { get; set; }

		public abstract MarkerOptions GetOptions();
	}
}