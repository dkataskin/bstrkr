using System;

using bstrkr.core.spatial;

namespace bstrkr.core
{
	public class Vehicle
	{
		public string Id { get; set; }

		public string CarPlate { get; set; }

		public GeoPoint Location { get; set; }

		public float Heading { get; set; }

		public VehicleRouteInfo RouteInfo { get; set; }

		public VehicleTypes Type { get; set; }
	}
}