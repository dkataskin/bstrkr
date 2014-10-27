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

using bstrkr.core.interfaces;
using bstrkr.core.spatial;

namespace bstrkr.core.providers.bus13
{
	public class Bus13LiveDataProvider : ILiveDataProvider
	{
		private readonly string _endpoint;
		private readonly string _location;
		private readonly TimeSpan _updateInterval;
		private readonly IBus13RouteDataService _dataService;

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
			var routes = _dataService.GetRoutesAsync()
									 .ConfigureAwait(false)
									 .GetAwaiter()
									 .GetResult();

			_cancellationTokenSource = new CancellationTokenSource();
			_updateTask = Task.Factory.StartNew(
										() => this.UpdateInLoop(_dataService, routes, _updateInterval, _cancellationTokenSource.Token), 
										_cancellationTokenSource.Token);
		}

		public void Stop()
		{
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

					this.RaiseVehicleLocationsUpdatedEvent(response.VehicleLocations.ToDictionary(x => x.Key.Id, x => x.Value));
				} 
				catch (Exception e)
				{
				}
				finally
				{
					Task.Delay(sleepInterval).Wait();
				}
			}
		}

		private void RaiseVehicleLocationsUpdatedEvent(IDictionary<string, WaypointCollection> vehicleLocations)
		{
			if (this.VehicleLocationsUpdated != null)
			{
				this.VehicleLocationsUpdated(this, new VehicleLocationsUpdatedEventArgs(vehicleLocations));
			}
		}
	}
}