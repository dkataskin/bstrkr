using System;

using bstrkr.mvvm.maps;
using bstrkr.mvvm.viewmodels;
using bstrkr.mvvm.views;

namespace bstrkr.android.views
{
	public class VehicleMarkerManager : MvxMarkerManager
	{
		public VehicleMarkerManager(IMapView mapView) : base(mapView)
		{
		}

		protected override IVehicleMarker CreateMarker(object item)
		{
			return new VehicleMarker(item as VehicleViewModel);
		}
	}
}