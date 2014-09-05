using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using bstrkr.core;
using bstrkr.core.spatial;

namespace bstrkr.core.providers.bus13
{
	public interface IBus13RouteDataService
	{
		Task<IEnumerable<Route>> GetRoutesAsync();

		Task<IEnumerable<RouteStop>> GetRouteStopsAsync(Route route);

		Task<GeoPolyline> GetRouteNodesAsync(Route route);

		Task<IEnumerable<Vehicle>> GetVehicleLocationsAsync(IEnumerable<Route> routes, GeoRect rect, int timestamp);

		Task<IEnumerable<RouteStop>> GetStopsAsync();
	}
}