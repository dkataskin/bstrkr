using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Immutable;

using bstrkr.core;

namespace bstrkr.core.interfaces
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