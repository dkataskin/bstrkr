using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using bstrkr.core.spatial;

namespace bstrkr.core
{
    public class Route
    {
        public Route(
                string id,
                string name,
                string number,
                VehicleTypes vehicleType,
                IEnumerable<RouteStop> stops,
                IEnumerable<GeoPoint> nodes)
        {
            this.Id = id;
            this.Name = name;
            this.Number = number;

            this.Stops = new ReadOnlyCollection<RouteStop>(stops.ToList());
            this.Nodes = new ReadOnlyCollection<GeoPoint>(nodes.ToList());
            this.VehicleType = vehicleType;
        }

        public string Id { get; }

        public string Name { get; }

        public string Number { get; private set; }

        public IReadOnlyList<RouteStop> Stops { get; private set; }

        public IReadOnlyList<GeoPoint> Nodes { get; private set; }

        public VehicleTypes VehicleType { get; }

        public RouteStop FirstStop { get; set; }

        public RouteStop LastStop { get; set; }

        public object VendorInfo { get; set; }

        public override string ToString()
        {
            return $"[Route: Id={this.Id}, Name={this.Name}, Type={this.VehicleType}, First Stop={this.FirstStop}, Last Stop={this.LastStop}]";
        }
    }
}