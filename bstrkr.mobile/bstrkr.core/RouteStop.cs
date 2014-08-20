using System;

namespace bstrkr.core
{
	public class RouteStop
	{
		public RouteStop(string id, string name, string description, Location location)
		{
			this.Id = id;
			this.Name = name;
			this.Description = description;
			this.Location = location;
		}

		public string Id { get; private set; }

		public Location Location { get; private set; }

		public string Name { get; private set; }

		public string Description { get; private set; }
	}
}