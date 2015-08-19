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
		private readonly IDictionary<string, Route> _allRoutesCache = new Dictionary<string, Route>();

		private bool _isRunning;
		private Task _updateTask;
		private CancellationTokenSource _cancellationTokenSource;

		public Bus13LiveDataProvider(Area area, TimeSpan updateInterval)
		{
			this.Area = area;
			_endpoint = area.Endpoint;
			_location = area.Id;
			_updateInterval = updateInterval;

			_dataService = new Bus13RouteDataService(_endpoint, _location);
		}

		public Area Area { get; private set; }

		public event EventHandler<VehicleLocationsUpdatedEventArgs> VehicleLocationsUpdated;

		public event EventHandler<RouteStopForecastReceivedEventArgs> RouteStopForecastReceived;

		public event EventHandler<VehicleForecastReceivedEventArgs> VehicleForecastReceived;

		public void Start()
		{
			lock(_lockObject)
			{
				if (!_isRunning)
				{
					_isRunning = true;

					MvxTrace.Trace(() => "started retrieving routes");
					this.CreateStartTask();
				}
			}
		}

		public void Stop()
		{
			lock(_lockObject)
			{
				if (_isRunning)
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

				_isRunning = false;
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
					_allRoutesCache[route.Id] = route;
					if (route.Ids != null)
					{
						foreach (var routeId in route.Ids)
						{
							_allRoutesCache[routeId] = route;
						}
					}
				}
			}

			return routes;
		}

		public async Task<Route> GetRouteAsync(string routeId)
		{
			try
			{
				var routes = await this.GetRoutesAsync();
				return routes.FirstOrDefault(x => x.Ids.Contains(routeId));
			} 
			catch (Exception e)
			{
				Insights.Report(e, Insights.Severity.Warning);
				return null;
			}
		}

		public async Task<IEnumerable<RouteStop>> GetRouteStopsAsync()
		{
			return await _dataService.GetStopsAsync().ConfigureAwait(false);
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
			return response.Updates
						   .Select(x => x.Vehicle)
						   .Where(v => v.RouteInfo != null && ids.Any(id => id.Equals(v.RouteInfo.RouteId)))
						   .ToList();
		}

		public async Task<VehicleForecast> GetVehicleForecastAsync(Vehicle vehicle)
		{
			var forecast = await _dataService.GetVehicleForecastAsync(vehicle).ConfigureAwait(false);

			this.RaiseVehicleForecastReceivedEvent(vehicle.Id, forecast);

			return forecast;
		}

		public async Task<RouteStopForecast> GetRouteStopForecastAsync(string routeStopId)
		{
			var forecast = await _dataService.GetRouteStopForecastAsync(routeStopId);
			lock(_routesCache)
			{
				foreach(var forecastItem in forecast.Items)
				{
					if (_allRoutesCache.ContainsKey(forecastItem.Route.Id))
					{
						forecastItem.ParentRoute = _allRoutesCache[forecastItem.Route.Id];
					}
				}
			}

			this.RaiseRouteStopForecastReceivedEvent(routeStopId, forecast);
			return forecast;
		}

		private void CreateStartTask()
		{
			if (_updateTask == null)
			{
				this.GetRoutesAsync()
					.ContinueWith(this.OnGetRoutesTaskCompleted)
					.ConfigureAwait(false);
			}
		}

		private void OnGetRoutesTaskCompleted(Task<IEnumerable<Route>> task)
		{
			if (task.Status == TaskStatus.RanToCompletion)
			{
				var routes = task.Result.ToList();
				if (routes.Any())
				{
					MvxTrace.Trace("{0} routes retrieved", routes.Count);

					_cancellationTokenSource = new CancellationTokenSource();
					_updateTask = Task.Factory.StartNew(
						() => this.UpdateInLoop(_dataService, routes, _updateInterval, _cancellationTokenSource.Token), 
						_cancellationTokenSource.Token);
				}
			}
			else
			{
				MvxTrace.Trace("an exception occured while retrieving routes: {0}", task.Exception);
				Insights.Report(task.Exception, Xamarin.Insights.Severity.Warning);

				Task.Delay(TimeSpan.FromSeconds(5)).Wait();
				this.CreateStartTask();
			}
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
					Insights.Report(e, Xamarin.Insights.Severity.Warning);
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
					vehicleLocationUpdates.Add(new VehicleLocationUpdate(
																update.Vehicle,
																new WaypointCollection(update.Waypoints),
																update.LastUpdate));
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

		private void RaiseRouteStopForecastReceivedEvent(string routeStopId, RouteStopForecast forecast)
		{
			if (this.RouteStopForecastReceived != null)
			{
				this.RouteStopForecastReceived(this, new RouteStopForecastReceivedEventArgs(routeStopId, forecast));
			}
		}

		private void RaiseVehicleForecastReceivedEvent(string vehicleId, VehicleForecast forecast)
		{
			if (this.VehicleForecastReceived != null)
			{
				this.VehicleForecastReceived(this, new VehicleForecastReceivedEventArgs(vehicleId, forecast));
			}
		}
	}
}