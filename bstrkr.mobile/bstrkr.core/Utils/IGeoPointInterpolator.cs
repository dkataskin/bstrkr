using bstrkr.core.spatial;

namespace bstrkr.core.utils
{
    public interface IGeoPointInterpolator
    {
        GeoPoint Interpolate(float fraction, GeoPoint a, GeoPoint b);
    }
}