using bstrkr.core.spatial;

namespace bstrkr.core
{
    public class RouteStop
    {
        public RouteStop(string id, string name, string description, GeoPoint location)
        {
            this.Id = id;
            this.Name = name;
            this.Description = description;
            this.Location = new GeoLocation(location);
        }

        public string Id { get; }

        public GeoLocation Location { get; }

        public string Name { get; }

        public string Description { get; private set; }

        public object VendorInfo { get; set; }

        public override string ToString()
        {
            return $"[RouteStop: Id={this.Id}, Name={this.Name}, Location={this.Location}]";
        }
    }
}