using System;
using System.Collections.Generic;

namespace bstrkr.core
{
	public interface ILiveDataProvider
	{
		IList<Route> GetRoutes();

		IList<Vehicle> GetVehicles();
	}
}