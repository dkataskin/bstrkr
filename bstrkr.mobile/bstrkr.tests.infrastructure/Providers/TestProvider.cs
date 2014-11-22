using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

using bstrkr.core;
using bstrkr.providers;

namespace bstrkr.tests.infrastructure.providers
{
	public class TestProvider : ILiveDataProvider
	{
		public TestProvider()
		{
		}

		public event EventHandler<VehicleLocationsUpdatedEventArgs> VehicleLocationsUpdated;

		public Task<IEnumerable<Route>> GetRoutesAsync()
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<RouteStop>> GetRouteStopsAsync()
		{
			throw new NotImplementedException();
		}

		public void Start()
		{
		}

		public void Stop()
		{
		}
	}
}