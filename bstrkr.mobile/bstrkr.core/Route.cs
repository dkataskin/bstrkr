using System;
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
				IEnumerable<string> ids,
				string name, 
				string number,
				IEnumerable<RouteStop> stops,
				IEnumerable<GeoPoint> nodes,
				IEnumerable<VehicleTypes> vehicleTypes)
		{
			this.Id = id;
			this.Name = name;
			this.Number = number;

			if (ids == null || !ids.Any())
			{
				this.Ids = new ReadOnlyCollection<string>(new List<string> { id });
			}
			else
			{
				this.Ids = new ReadOnlyCollection<string>(ids.ToList());
			}

			this.Stops = new ReadOnlyCollection<RouteStop>(stops.ToList());
			this.Nodes = new ReadOnlyCollection<GeoPoint>(nodes.ToList());
			this.VehicleTypes = new ReadOnlyCollection<VehicleTypes>(vehicleTypes.ToList());
		}

		public string Id { get; private set; }

		public string Name { get; private set; }

		public string Number { get; private set; }

		public IReadOnlyList<string> Ids { get; private set; }

		public IReadOnlyList<RouteStop> Stops { get; private set; }

		public IReadOnlyList<GeoPoint> Nodes { get; private set; }

		public IReadOnlyList<VehicleTypes> VehicleTypes { get; private set; }

		public RouteStop FirstStop { get; set; }

		public RouteStop LastStop { get; set; }

		public override string ToString()
		{
			return string.Format(
					"[Route: Id={0}, Name={1}, Type={2}, First Stop={3}, Last Stop={4}]", 
					this.Id,
					this.Name,
					this.VehicleTypes == null ? "none" : string.Join(":", this.VehicleTypes.Select(x => x.ToString()).ToArray()),
					this.FirstStop,
					this.LastStop);
		}
	}
}