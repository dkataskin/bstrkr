using System;
using System.Collections.Immutable;

using bstrkr.core;
using bstrkr.core.interfaces;

namespace bstrkr.tests.providers
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
	}
}