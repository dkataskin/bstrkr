using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using bstrkr.core;

namespace bstrkr.providers
{
	public interface ILiveDataProvider
	{
		Area Area { get; }

		event EventHandler<VehicleLocationsUpdatedEventArgs> VehicleLocationsUpdated;

		Task<IEnumerable<Route>> GetRoutesAsync();

		Task<Route> GetRouteAsync(string routeId);

		Task<IEnumerable<RouteStop>> GetRouteStopsAsync();

		Task<IEnumerable<Vehicle>> GetVehiclesAsync();

		Task<IEnumerable<Vehicle>> GetRouteVehiclesAsync(IEnumerable<Route> routes);

		Task<VehicleForecast> GetVehicleForecastAsync(Vehicle vehicle);

		Task<RouteStopForecast> GetRouteStopForecastAsync(string routeStopId);

		void Start();

		void Stop();
	}
}