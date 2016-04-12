using bstrkr.mvvm.viewmodels;

namespace bstrkr.mvvm.views
{
    public interface IVehicleMarker : IMapMarker
    {
        VehicleViewModel ViewModel { get; }
    }
}