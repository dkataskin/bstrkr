using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace bstrkr.core
{
	public interface ILiveDataProvider
	{
		event EventHandler<VehicleLocationsUpdatedEventArgs> VehicleLocationsUpdated;

		void Start();

		void Stop();
	}
}