using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using bstrkr.core;

namespace bstrkr.providers.bus13.data
{
	public class VehicleLocationsResponse
	{
		public VehicleLocationsResponse(int timestamp, IDictionary<Vehicle, WaypointCollection> vehicleLocations)
		{
			this.Timestamp = timestamp;
			this.VehicleLocations = new ReadOnlyDictionary<Vehicle, WaypointCollection>(vehicleLocations);
		}

		public int Timestamp { get; private set; }

		public IReadOnlyDictionary<Vehicle, WaypointCollection> VehicleLocations { get; private set; }
	}
}