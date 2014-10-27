using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace bstrkr.core
{
	public class VehicleLocationsUpdatedEventArgs : EventArgs
	{
		public VehicleLocationsUpdatedEventArgs(IDictionary<string, WaypointCollection> vehicleLocations)
		{
			this.VehicleLocations = new ReadOnlyDictionary<string, WaypointCollection>(vehicleLocations);
		}

		public IReadOnlyDictionary<string, WaypointCollection> VehicleLocations { get; private set; }
	}
}