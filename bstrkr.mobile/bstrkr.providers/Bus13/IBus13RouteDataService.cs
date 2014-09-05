using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using bstrkr.core;

namespace bstrkr.core.providers.bus13
{
	public interface IBus13RouteDataService
	{
		Task<IEnumerable<Route>> GetRoutesAsync();

		Task<IEnumerable<GeoPoint>> GetRouteNodes(string routeId);

		Task<IEnumerable<Vehicle>> GetVehicleLocationsAsync(IEnumerable<Route> routes, GeoRect rect, int timestamp);

		Task<IEnumerable<RouteStop>> GetRouteStopsAsync(Route route);

		Task<IEnumerable<RouteStop>> GetStopsAsync();
	}
}