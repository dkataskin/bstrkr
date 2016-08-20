using System;
using bstrkr.mvvm.maps;
using bstrkr.mvvm.viewmodels;
using bstrkr.mvvm.views;

namespace bstrkr.android.views
{
    public class VehicleMarkerManager : MapMarkerManager
    {
        private MapVehiclesViewModel _viewModel;
        private IDisposable _subscription;

        public VehicleMarkerManager(IMapView mapView, MapVehiclesViewModel viewModel) : base(mapView)
        {
            _subscription = viewModel.ViewportUpdate.Subscribe(this.UpdateVehiclesInView);
        }

        protected override IMapMarker CreateMarker(object item)
        {
            return new VehicleMarker(item as VehicleViewModel);
        }

        private void UpdateVehiclesInView(VisibleVehiclesDelta delta)
        {
            this.RemoveMarkers(delta.VehiclesToRemove);
            this.AddMarkers(delta.VehiclesToAdd);
        }
    }
}