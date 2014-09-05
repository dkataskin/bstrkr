using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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

		public IImmutableList<Route> Routes 
		{
			get 
			{
				throw new NotImplementedException();
			}
		}

		public IImmutableList<Vehicle> Vehicles 
		{
			get 
			{
				throw new NotImplementedException();
			}
		}

		public void Start()
		{
			//_dataService.GetRoutesAsync();

			_cancellationTokenSource = new CancellationTokenSource();
			_updateTask = Task.Factory.StartNew(
										() => this.UpdateInLoop(_dataService, _updateInterval, _cancellationTokenSource.Token), 
										_cancellationTokenSource.Token);
		}

		public void Stop()
		{
		}

		private void UpdateInLoop(IBus13RouteDataService dataService, TimeSpan sleepInterval, CancellationToken token)
		{
			while(!token.IsCancellationRequested)
			{
				try
				{
					//_dataService.GetVehicleLocationsAsync();
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

		private void RaiseVehicleLocationsUpdatedEvent(IEnumerable<Vehicle> vehicles)
		{
			if (this.VehicleLocationsUpdated != null)
			{
				this.VehicleLocationsUpdated(this, new VehicleLocationsUpdatedEventArgs(vehicles));
			}
		}
	}
}