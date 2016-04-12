namespace bstrkr.core.spatial
{
    public struct GeoLocation
    {
        public GeoPoint Position;
        public float Heading;

        public GeoLocation(GeoPoint position) : this(position, 0)
        {
        }

        public GeoLocation(GeoPoint position, float heading)
        {
            this.Position = position;
            this.Heading = heading;
        }

        public static GeoLocation Empty = new GeoLocation(GeoPoint.Empty, 0);

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            var geoLocation = (GeoLocation)obj;
            return this.Position.Equals(geoLocation.Position) &&
                   this.Heading.Equals(geoLocation.Heading);
        }

        public override string ToString()
        {
            return $"[{this.Position}, heading: {this.Heading:F2}]";
        }
    }
}