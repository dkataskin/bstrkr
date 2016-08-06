using System;
using bstrkr.mvvm.maps;
using bstrkr.mvvm.viewmodels;
using bstrkr.mvvm.views;

namespace bstrkr.android.views
{
    public class RouteStopMarkerManager : MapMarkerManager
    {
        private MapRouteStopsViewModel _viewModel;

        public RouteStopMarkerManager(IMapView mapView, MapRouteStopsViewModel viewModel) : base(mapView)
        {
            _viewModel = viewModel;
            _viewModel.RouteStopCollectionChanged += OnRouteStopCollectionChanged;
        }

        protected override IMapMarker CreateMarker(object item)
        {
            return new RouteStopMarker(item as RouteStopMapViewModel);
        }

        private void OnRouteStopCollectionChanged(object sender, EventArgs e)
        {
            this.RemoveAllMarkers();
            this.AddMarkers(_viewModel.Stops);
        }
    }
}