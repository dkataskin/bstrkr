using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace bstrkr.core
{
	public class VehicleForecast
	{
		public VehicleForecast(Vehicle vehicle, IEnumerable<VehicleForecastItem> items)
		{
			this.Vehicle = vehicle;
			this.Items = new ReadOnlyCollection<VehicleForecastItem>();
		}

		public Vehicle Vehicle { get; private set; }

		public IReadOnlyList<VehicleForecastItem> Items { get; private set; }
	}
}