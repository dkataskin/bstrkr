using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Immutable;

namespace bstrkr.core
{
	public interface ILiveDataProvider
	{
		event EventHandler<VehicleLocationsUpdatedEventArgs> VehicleLocationsUpdated;

		IImmutableList<Route> Routes { get; }

		IImmutableList<Vehicle> Vehicles { get; }

		void Start();

		void Stop();
	}
}