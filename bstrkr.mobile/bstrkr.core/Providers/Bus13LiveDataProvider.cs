using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

using Newtonsoft.Json.Linq;

using RestSharp.Portable;
using RestSharp.Portable.Deserializers;
using Providers;

namespace bstrkr.core.providers
{
	public class Bus13LiveDataProvider : ILiveDataProvider
	{
		private string _endpoint;
		private string _location;

		private IBus13RouteDataService _dataService;

		public Bus13LiveDataProvider(string endpoint, string location)
		{
			_endpoint = endpoint;
			_location = location;

			_dataService = new Bus13RouteDataService(_endpoint, _location);
		}

		public event EventHandler<VehicleLocationsUpdatedEventArgs> VehicleLocationsUpdated;

		public void Start()
		{
		}

		public void Stop()
		{
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