using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using bstrkr.core;

namespace bstrkr.providers.bus13.data
{
	public class VehicleLocationsResponse
	{
		public VehicleLocationsResponse(int timestamp, IEnumerable<Bus13VehicleLocationUpdate> vehicleLocations)
		{
			this.Timestamp = timestamp;
			this.Updates = new ReadOnlyCollection<Bus13VehicleLocationUpdate>(vehicleLocations.ToList());
		}

		public int Timestamp { get; private set; }

		public IReadOnlyCollection<Bus13VehicleLocationUpdate> Updates { get; private set; }
	}
}