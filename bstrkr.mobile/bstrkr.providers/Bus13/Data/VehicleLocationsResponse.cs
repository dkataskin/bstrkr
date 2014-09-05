using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using bstrkr.core;

namespace bstrkr.providers.bus13.data
{
	public class VehicleLocationsResponse
	{
		public VehicleLocationsResponse(int timestamp, IEnumerable<Vehicle> vehicles)
		{
			this.Timestamp = timestamp;
			this.Vehicles = new ReadOnlyCollection<Vehicle>(vehicles.ToList());
		}

		public int Timestamp { get; private set; }

		public IReadOnlyCollection<Vehicle> Vehicles { get; private set; }
	}
}