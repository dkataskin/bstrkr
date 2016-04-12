using Android.Gms.Maps.Model;

using bstrkr.core.spatial;

namespace bstrkr.core.android.extensions
{
    public static class GeoPointExtensions
    {
        public static LatLng ToLatLng(this GeoPoint point)
        {
            return new LatLng(point.Latitude, point.Longitude);
        }
    }
}