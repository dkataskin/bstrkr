using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace bstrkr.core
{
	public class VehicleLocationsUpdatedEventArgs : EventArgs
	{
		public VehicleLocationsUpdatedEventArgs(IEnumerable<Vehicle> vehicles)
		{
			this.Vehicles = new ReadOnlyCollection<Vehicle>(vehicles.ToList());
		}

		public IReadOnlyCollection<Vehicle> Vehicles { get; private set; }
	}
}