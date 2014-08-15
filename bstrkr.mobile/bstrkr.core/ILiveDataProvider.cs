using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace bstrkr.core
{
	public interface ILiveDataProvider
	{
		IEnumerable<Route> GetRoutes();

		Task<IEnumerable<Route>> GetRoutesAsync();

		IEnumerable<Vehicle> GetVehicles();

		Task<IEnumerable<Vehicle>> GetVehiclesAsync();
	}
}