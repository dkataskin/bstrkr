using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

using bstrkr.core;

namespace bstrkr.core.interfaces
{
	public interface ILiveDataProvider
	{
		event EventHandler<VehicleLocationsUpdatedEventArgs> VehicleLocationsUpdated;

		Task<IImmutableList<Route>> GetRoutesAsync();

		Task<IImmutableList<RouteStop>> GetRouteStopsAsync();

		void Start();

		void Stop();
	}
}