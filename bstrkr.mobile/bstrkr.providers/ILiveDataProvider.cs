using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using bstrkr.core;

namespace bstrkr.providers
{
	public interface ILiveDataProvider
	{
		event EventHandler<VehicleLocationsUpdatedEventArgs> VehicleLocationsUpdated;

		Task<IEnumerable<Route>> GetRoutesAsync();

		Task<IEnumerable<RouteStop>> GetRouteStopsAsync();

		Task<IEnumerable<Vehicle>> GetVehiclesAsync();

		Task<IEnumerable<Vehicle>> GetRouteVehiclesAsync(IEnumerable<Route> routes);

		void Start();

		void Stop();
	}
}