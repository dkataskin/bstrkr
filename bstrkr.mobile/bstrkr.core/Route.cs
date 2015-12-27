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

		public string Id { get; private set; }

		public string Name { get; private set; }

		public string Number { get; private set; }

		public IReadOnlyList<RouteStop> Stops { get; private set; }

		public IReadOnlyList<GeoPoint> Nodes { get; private set; }

		public VehicleTypes VehicleType { get; private set; }

		public RouteStop FirstStop { get; set; }

		public RouteStop LastStop { get; set; }

		public object VendorInfo { get; set; }

		public override string ToString()
		{
			return string.Format(
					"[Route: Id={0}, Name={1}, Type={2}, First Stop={3}, Last Stop={4}]", 
					this.Id,
					this.Name,
					this.VehicleType.ToString(),
					this.FirstStop,
					this.LastStop);
		}
	}
}