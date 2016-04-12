using bstrkr.core.spatial;

namespace bstrkr.core
{
    public struct Waypoint
    {
        public float Fraction;
        public GeoLocation Location;

        public Waypoint(GeoLocation location, float fraction)
        {
            this.Location = location;
            this.Fraction = fraction;
        }

        public Waypoint(GeoPoint position, float heading, float fraction)
        {
            this.Location = new GeoLocation(position, heading);
            this.Fraction = fraction;
        }
    }
}