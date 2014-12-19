using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

using Newtonsoft.Json.Linq;

using RestSharp.Portable;
using RestSharp.Portable.Deserializers;

using Xamarin;

using bstrkr.core.spatial;
using bstrkr.providers;
using bstrkr.providers.bus13.data;

namespace bstrkr.core.providers.bus13
{
	public class Bus13LiveDataProvider : ILiveDataProvider
	{
		private readonly string _endpoint;
		private readonly string _location;
		private readonly TimeSpan _updateInterval;
		private readonly IBus13RouteDataService _dataService;
		private readonly IDictionary<string, Bus13VehicleLocationUpdate> _locationState = new Dictionary<string, Bus13VehicleLocationUpdate>();

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
			var routes = new List<Route>();
			try
			{
				routes = _dataService.GetRoutesAsync()
								     .ConfigureAwait(false)
									 .GetAwaiter()
									 .GetResult()
									 .ToList();
			} 
			catch (Exception e)
			{
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
			return await _dataService.GetRoutesAsync();
		}

		public async Task<IEnumerable<RouteStop>> GetRouteStopsAsync()
		{
			return await _dataService.GetStopsAsync();
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
			var vehicleLocationUpdates = new List<VehicleLocationUpdate>();
			foreach (var update in updates)
			{
				if (!_locationState.ContainsKey(update.Vehicle.Id))
				{
					_locationState[update.Vehicle.Id] = update;
				}
				else
				{
					_locationState[update.Vehicle.Id] = update;
				}

				vehicleLocationUpdates.Add(new VehicleLocationUpdate(update.Vehicle, new WaypointCollection()));
			}

			return vehicleLocationUpdates;
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