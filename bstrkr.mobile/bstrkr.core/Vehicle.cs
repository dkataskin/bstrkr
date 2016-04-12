using bstrkr.core.spatial;

namespace bstrkr.core
{
    public class Vehicle
    {
        public string Id { get; set; }

        public string CarPlate { get; set; }

        public GeoLocation Location { get; set; }

        public VehicleRouteInfo RouteInfo { get; set; }

        public VehicleTypes Type { get; set; }
    }
}