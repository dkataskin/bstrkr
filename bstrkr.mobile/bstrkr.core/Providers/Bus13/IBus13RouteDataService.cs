using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using bstrkr.core;

namespace bstrkr.core.providers.bus13
{
	public interface IBus13RouteDataService
	{
		Task<IEnumerable<Route>> GetRoutesAsync();

		Task<IEnumerable<Vehicle>> GetVehicleLocationsAsync(IEnumerable<Route> routes, Rect rect, int timestamp);

		Task<IEnumerable<RouteStop>> GetRouteStopsAsync(Route route);

		Task<IEnumerable<RouteStop>> GetStopsAsync();
	}
}