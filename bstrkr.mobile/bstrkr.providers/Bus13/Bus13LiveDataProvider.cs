using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

using bstrkr.core.spatial;
using bstrkr.providers;
using bstrkr.providers.bus13.data;

//using Cirrious.CrossCore.Platform;

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

		private bool _isRunning;
		private UpdateRoutineState _updateRoutineState;
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

		public event EventHandler<EventArgs> VehicleLocationsUpdateStarted;

		public event EventHandler<EventArgs> VehicleLocationsUpdateFailed;

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

//					MvxTrace.Trace(() => "started retrieving routes");
					this.CreateStartTask();
				}
			}
		}

		public Task UpdateVehicleLocationsAsync()
		{
			if (_updateRoutineState == null)
			{
				return Task.FromResult<object>(null);
			}

			return Task.Factory.StartNew(() => 
			{
				lock(_lockObject)
				{
					try
					{
						_updateRoutineState = this.RunUpdateVehicleLocationsRoutine(_dataService, _updateRoutineState);
					} 
					catch (Exception e)
					{
						this.RaiseVehicleLocationsUpdateFailedEvent();
						Insights.Report(e, Xamarin.Insights.Severity.Warning);
					}
				}
			});
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

					_updateTask = null;
					_cancellationTokenSource = null;
				}

				_isRunning = false;
			}
		}

		public async Task<IEnumerable<Route>> GetRoutesAsync(bool noCache = false)
		{
			if (!noCache)
			{
				lock(_routesCache)
				{
					if (_routesCache.Count != 0)
					{
						return _routesCache.Values.ToList();
					}
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

		public async Task<Route> GetRouteAsync(string routeId)
		{
			try
			{
				var routes = await this.GetRoutesAsync();
				return routes.FirstOrDefault(x => x.Id.Equals(routeId));
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
			var ids = routes.Select(r => r.Id).ToList();
			lock(_locationState)
			{
				if (_locationState.Keys.Any())
				{
					return _locationState.Values.Select(v => v.Vehicle)
												.Where(v => v.RouteInfo != null && ids.Contains(v.RouteInfo.RouteId))
												.ToList();
				}
			}

			var response = await _dataService.GetVehicleLocationsAsync(routes, GeoRect.EarthWide, 0);
			return response.Updates
						   .Select(x => x.Vehicle)
						   .Where(v => v.RouteInfo != null && ids.Contains(v.RouteInfo.RouteId))
						   .ToList();
		}

		public async Task<VehicleForecast> GetVehicleForecastAsync(Vehicle vehicle)
		{
			return await _dataService.GetVehicleForecastAsync(vehicle).ConfigureAwait(false);
		}

		public async Task<RouteStopForecast> GetRouteStopForecastAsync(RouteStop routeStop)
		{
			var forecast = await _dataService.GetRouteStopForecastAsync(routeStop);
			lock(_routesCache)
			{
				foreach(var forecastItem in forecast.Items)
				{
					if (_routesCache.ContainsKey(forecastItem.Route.Id))
					{
						forecastItem.Route = _routesCache[forecastItem.Route.Id];
					}
				}
			}

			return forecast;
		}

		private void CreateStartTask()
		{
			this.GetRoutesAsync()
				.ContinueWith(this.OnGetRoutesTaskCompleted)
				.ConfigureAwait(false);
		}

		private void OnGetRoutesTaskCompleted(Task<IEnumerable<Route>> task)
		{
			if (task.Status == TaskStatus.RanToCompletion)
			{
				var routes = task.Result.ToList();
				if (routes.Any())
				{
//					MvxTrace.Trace("{0} routes retrieved", routes.Count);

					if (_cancellationTokenSource != null)
					{
						_cancellationTokenSource.Cancel();
					}

					_cancellationTokenSource = new CancellationTokenSource();
					_updateTask = Task.Factory.StartNew(
						() => this.UpdateInLoop(_dataService, routes, _updateInterval, _cancellationTokenSource.Token), 
						_cancellationTokenSource.Token);
				}
			}
			else
			{
//				MvxTrace.Trace("an exception occured while retrieving routes: {0}", task.Exception);
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
			_updateRoutineState = new UpdateRoutineState(routes);
			while(!token.IsCancellationRequested)
			{
				var nextSleepInterval = sleepInterval;
				lock(_lockObject)
				{
					var now = DateTime.UtcNow;
					try
					{
						if ((now - _updateRoutineState.LastUpdate) >= sleepInterval)
						{
							_updateRoutineState = this.RunUpdateVehicleLocationsRoutine(dataService, _updateRoutineState);
						}
						else
						{
							nextSleepInterval = sleepInterval - (now - _updateRoutineState.LastUpdate) + TimeSpan.FromSeconds(1);
						}
					} 
					catch (Exception e)
					{
						this.RaiseVehicleLocationsUpdateFailedEvent();
						Insights.Report(e, Xamarin.Insights.Severity.Warning);
					}
				}

				Task.Delay(nextSleepInterval).Wait(token);
			}
		}

		private UpdateRoutineState RunUpdateVehicleLocationsRoutine(
												IBus13RouteDataService dataService,
												UpdateRoutineState state)
		{
			this.RaiseVehicleLocationsUpdateStartedEvent();

			var response = _dataService.GetVehicleLocationsAsync(
															state.Routes, 
															GeoRect.EarthWide, 
															state.Timestamp)
										.ConfigureAwait(false)
										.GetAwaiter()
										.GetResult();

			var vehicleLocationUpdates = this.UpdateVehicleLocations(response.Updates);

			this.RaiseVehicleLocationsUpdatedEvent(vehicleLocationUpdates);

			return UpdateRoutineState.CreateFrom(state, response.Timestamp);
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

		private void RaiseVehicleLocationsUpdateStartedEvent()
		{
			if (this.VehicleLocationsUpdateStarted != null)
			{
				this.VehicleLocationsUpdateStarted(this, EventArgs.Empty);
			}
		}

		private void RaiseVehicleLocationsUpdateFailedEvent()
		{
			if (this.VehicleLocationsUpdateFailed != null)
			{
				this.VehicleLocationsUpdateFailed(this, EventArgs.Empty);
			}
		}

		private void RaiseVehicleLocationsUpdatedEvent(IEnumerable<VehicleLocationUpdate> vehicleLocations)
		{
			if (this.VehicleLocationsUpdated != null)
			{
				this.VehicleLocationsUpdated(this, new VehicleLocationsUpdatedEventArgs(vehicleLocations));
			}
		}

		private class UpdateRoutineState
		{
			public UpdateRoutineState(IEnumerable<Route> routes)
			{
				this.LastUpdate = DateTime.MinValue;
				this.Routes = routes;
			}

			public int Timestamp { get; set; }

			public DateTime LastUpdate { get; set; }

			public IEnumerable<Route> Routes { get; set; } 

			public static UpdateRoutineState CreateFrom(UpdateRoutineState state, int timestamp)
			{
				return new UpdateRoutineState(state.Routes)
				{
					Timestamp = timestamp,
					LastUpdate = DateTime.UtcNow
				};
			}
		}
	}
}