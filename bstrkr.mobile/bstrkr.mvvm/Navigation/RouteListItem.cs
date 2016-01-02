using System;

using bstrkr.core;

namespace bstrkr.mvvm.navigation
{
	public class RouteListItem
	{
		public RouteListItem()
		{
		}

		public RouteListItem(string id, string name, string number, string stops, VehicleTypes vehicleType)
		{
			this.Id = id;
			this.Name = name;
			this.Number = number;
			this.Stops = stops;
			this.VehicleType = vehicleType;
		}

		public string Id { get; set; }

		public string Number { get; set; }

		public string Name { get; set; }

		public string Stops { get; set; }

		public VehicleTypes VehicleType { get; set; }
	}
}