using bstrkr.mvvm.maps;
using bstrkr.mvvm.viewmodels;
using bstrkr.mvvm.views;

namespace bstrkr.android.views
{
    public class RouteStopMarkerManager : MapMarkerManager
    {
        public RouteStopMarkerManager(IMapView mapView) : base(mapView)
        {
        }

        protected override IMapMarker CreateMarker(object item)
        {
            return new RouteStopMarker(item as RouteStopMapViewModel);
        }
    }
}