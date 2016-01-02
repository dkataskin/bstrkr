using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using bstrkr.core;

namespace bstrkr.providers
{
	public interface ILiveDataProvider
	{
		Area Area { get; }

		event EventHandler<EventArgs> VehicleLocationsUpdateStarted;

		event EventHandler<EventArgs> VehicleLocationsUpdateFailed;

		event EventHandler<VehicleLocationsUpdatedEventArgs> VehicleLocationsUpdated;

		Task<IEnumerable<Route>> GetRoutesAsync(bool noCache = false);

		Task<Route> GetRouteAsync(string routeId);

		Task<IEnumerable<RouteStop>> GetRouteStopsAsync();

		Task<IEnumerable<Vehicle>> GetVehiclesAsync();

		Task<IEnumerable<Vehicle>> GetRouteVehiclesAsync(IEnumerable<Route> routes);

		Task<VehicleForecast> GetVehicleForecastAsync(Vehicle vehicle);

		Task<RouteStopForecast> GetRouteStopForecastAsync(RouteStop routeStop);

		void Start();

		Task UpdateVehicleLocationsAsync();

		void Stop();
	}
}