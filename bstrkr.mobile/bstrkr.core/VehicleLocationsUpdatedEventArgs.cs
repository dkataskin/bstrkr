﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace bstrkr.core
{
	public class VehicleLocationsUpdatedEventArgs : EventArgs
	{
		public VehicleLocationsUpdatedEventArgs(IEnumerable<VehicleLocationUpdate> vehicleLocationUpdates)
		{
			this.VehicleLocations = new ReadOnlyCollection<VehicleLocationUpdate>(vehicleLocationUpdates.ToList());
		}

		public IReadOnlyList<VehicleLocationUpdate> VehicleLocations { get; private set; }
	}
}