using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using bstrkr.core;

namespace bstrkr.providers.bus13.data
{
	public class VehicleLocationsResponse
	{
		public VehicleLocationsResponse(int timestamp, IEnumerable<VehicleLocationUpdate> vehicleLocations)
		{
			this.Timestamp = timestamp;
			this.VehicleLocations = new ReadOnlyCollection<VehicleLocationUpdate>(vehicleLocations.ToList());
		}

		public int Timestamp { get; private set; }

		public IReadOnlyCollection<VehicleLocationUpdate> VehicleLocations { get; private set; }
	}
}