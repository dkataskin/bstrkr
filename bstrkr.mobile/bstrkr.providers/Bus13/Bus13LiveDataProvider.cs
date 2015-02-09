using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

using bstrkr.core.spatial;
using bstrkr.providers;
using bstrkr.providers.bus13.data;

using Cirrious.CrossCore.Platform;

using Newtonsoft.Json.Linq;

using RestSharp.Portable;
using RestSharp.Portable.Deserializers;

using Xamarin;

namespace bstrkr.core.providers.bus13
{
	public class Bus13LiveDataProvider : ILiveDataProvider
	{
		private readonly object _lockObject = new object();
		private readonly string _endpoint;
		private readonly string _location;
		private readonly TimeSpan _updateInterval;
		private readonly IBus13RouteDataService _dataService;
		private readonly IDictionary<string, Bus13VehicleLocationUpdate> _locationState = new Dictionary<string, Bus13VehicleLocationUpdate>();
		private readonly IDictionary<string, Route> _routesCache = new Dictionary<string, Route>();

		private Task _updateTask;
		private CancellationTokenSource _cancellationTokenSource;

		public Bus13LiveDataProvider(string endpoint, string location, TimeSpan updateInterval)
		{
			_endpoint = endpoint;
			_location = location;
			_updateInterval = updateInterval;

			_dataService = new Bus13RouteDataService(_endpoint, _location);
		}

		public event EventHandler<VehicleLocationsUpdatedEventArgs> VehicleLocationsUpdated;

		public void Start()
		{
			MvxTrace.Trace(() => "started retrieving routes");
			var routes = new List<Route>();
			try
			{
				routes = _dataService.GetRoutesAsync()
								     .ConfigureAwait(false)
									 .GetAwaiter()
									 .GetResult()
									 .ToList();

				MvxTrace.Trace("{0} routes retrieved", routes.Count);
			} 
			catch (Exception e)
			{
				MvxTrace.Trace("an exception occured while retrieving routes: {0}", e);
				Insights.Report(e, ReportSeverity.Warning);
			}

			if (routes.Any())
			{
				_cancellationTokenSource = new CancellationTokenSource();
				_updateTask = Task.Factory.StartNew(
						() => this.UpdateInLoop(_dataService, routes, _updateInterval, _cancellationTokenSource.Token), 
						_cancellationTokenSource.Token);
			}
		}

		public void Stop()
		{
			if (_cancellationTokenSource != null)
			{
				_cancellationTokenSource.Cancel();
			}

			if (_updateTask != null)
			{
				_cancellationTokenSource.Cancel();
			}
		}

		public async Task<IEnumerable<Route>> GetRoutesAsync()
		{
			lock(_routesCache)
			{
				if (_routesCache.Count != 0)
				{
					return _routesCache.Values.ToList();
				}
			}

			var routes = await _dataService.GetRoutesAsync();
			lock (_routesCache)
			{
				foreach (var route in routes)
				{
					_routesCache[route.Id] = route;
				}
			}

			return routes;
		}

		public async Task<IEnumerable<RouteStop>> GetRouteStopsAsync()
		{
			return await _dataService.GetStopsAsync();
		}

		public async Task<IEnumerable<Vehicle>> GetVehiclesAsync()
		{
			lock(_locationState)
			{
				if (_locationState.Keys.Any())
				{
					return _locationState.Values.Select(v => v.Vehicle);
				}
			}

			var routes = await this.GetRoutesAsync();
			var response = await _dataService.GetVehicleLocationsAsync(routes, GeoRect.EarthWide, 0);

			return response.Updates.Select(x => x.Vehicle);
		}

		public async Task<IEnumerable<Vehicle>> GetRouteVehiclesAsync(IEnumerable<Route> routes)
		{
			var ids = routes.SelectMany(r => r.Ids).ToList();
			lock(_locationState)
			{
				if (_locationState.Keys.Any())
				{
					return _locationState.Values.Select(v => v.Vehicle)
												.Where(v => v.RouteInfo != null && ids.Any(id => id.Equals(v.RouteInfo.RouteId)))
												.ToList();
				}
			}

			var response = await _dataService.GetVehicleLocationsAsync(routes, GeoRect.EarthWide, 0);

			return response.Updates.Select(x => x.Vehicle);
		}

		public async Task<VehicleForecast> GetVehicleForecastAsync(Vehicle vehicle)
		{
			return await _dataService.GetVehicleForecastAsync(vehicle);
		}

		private void UpdateInLoop(
							IBus13RouteDataService dataService, 
							IEnumerable<Route> routes, 
							TimeSpan sleepInterval, 
							CancellationToken token)
		{
			var timestamp = 0;
			while(!token.IsCancellationRequested)
			{
				try
				{
					var response = _dataService.GetVehicleLocationsAsync(routes, GeoRect.EarthWide, timestamp)
											   .ConfigureAwait(false)
											   .GetAwaiter()
											   .GetResult();

					timestamp = response.Timestamp;

					var vehicleLocationUpdates = this.UpdateVehicleLocations(response.Updates);

					this.RaiseVehicleLocationsUpdatedEvent(vehicleLocationUpdates);
				} 
				catch (Exception e)
				{
					Insights.Report(e, ReportSeverity.Warning);
				}
				finally
				{
					Task.Delay(sleepInterval).Wait();
				}
			}
		}

		private IEnumerable<VehicleLocationUpdate> UpdateVehicleLocations(IEnumerable<Bus13VehicleLocationUpdate> updates)
		{
			lock(_locationState)
			{
				var vehicleLocationUpdates = new List<VehicleLocationUpdate>();
				foreach (var update in updates)
				{
					_locationState[update.Vehicle.Id] = update;
					vehicleLocationUpdates.Add(new VehicleLocationUpdate(update.Vehicle, new WaypointCollection()));
				}

				return vehicleLocationUpdates;
			}
		}

		private void RaiseVehicleLocationsUpdatedEvent(IEnumerable<VehicleLocationUpdate> vehicleLocations)
		{
			if (this.VehicleLocationsUpdated != null)
			{
				this.VehicleLocationsUpdated(this, new VehicleLocationsUpdatedEventArgs(vehicleLocations));
			}
		}
	}
}