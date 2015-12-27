using System;

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

		public string Id { get; private set; }

		public GeoLocation Location { get; private set; }

		public string Name { get; private set; }

		public string Description { get; private set; }

		public object VendorInfo { get; set; }

		public override string ToString()
		{
			return string.Format(
						"[RouteStop: Id={0}, Name={1}, Location={2}]", 
						this.Id, 
						this.Name, 
						this.Location);
		}
	}
}