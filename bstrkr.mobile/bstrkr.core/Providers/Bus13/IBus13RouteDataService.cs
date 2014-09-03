using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using bstrkr.core;

namespace bstrkr.core.providers.bus13
{
	public interface IBus13RouteDataService
	{
		Task<IEnumerable<Route>> GetRoutesAsync();

		Task<IEnumerable<Vehicle>> GetVehicleLocationsAsync();

		Task<IEnumerable<RouteStop>> GetRouteStopsAsync(Route route);
	}
}