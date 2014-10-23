using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

using bstrkr.core;
using bstrkr.core.interfaces;

namespace bstrkr.tests.infrastructure.providers
{
	public class TestProvider : ILiveDataProvider
	{
		public TestProvider()
		{
		}

		public event EventHandler<VehicleLocationsUpdatedEventArgs> VehicleLocationsUpdated;

		public void Start()
		{
		}

		public void Stop()
		{
		}

		public Task<IEnumerable<Route>> GetRoutesAsync()
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<RouteStop>> GetRouteStopsAsync()
		{
			throw new NotImplementedException();
		}
	}
}