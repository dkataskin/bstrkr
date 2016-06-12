using bstrkr.mvvm.views;

namespace bstrkr.mvvm.maps
{
    public interface IMapMarkerManager
    {
        T GetDataForMarker<T>(IMapMarker marker);
    }
}