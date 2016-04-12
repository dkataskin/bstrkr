using Android.Gms.Maps.Model;

using bstrkr.core.spatial;

namespace bstrkr.core.android.extensions
{
    public static class LatLngExtensions
    {
        public static GeoPoint ToGeoPoint(this LatLng latLng)
        {
            return new GeoPoint(latLng.Latitude, latLng.Longitude);
        }
    }
}