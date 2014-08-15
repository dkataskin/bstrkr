using System;
using System.Collections.Generic;

namespace bstrkr.core.providers
{
	public class Bus13LiveDataProvider : ILiveDataProvider
	{
		private string _endpoint;

		public Bus13LiveDataProvider(string endpoint)
		{
			_endpoint = endpoint;
		}

		public IList<Route> GetRoutes()
		{
			throw new NotImplementedException ();
		}

		public IList<Vehicle> GetVehicles()
		{
			throw new NotImplementedException ();
		}
	}
}