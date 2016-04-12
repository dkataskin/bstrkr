using Android.Gms.Maps.Model;

using bstrkr.core.spatial;

namespace bstrkr.core.android.extensions
{
    public static class GeoLocationExtensions
    {
        public static LatLng ToLatLng(this GeoLocation location)
        {
            return location.Position.ToLatLng();
        }
    }
}