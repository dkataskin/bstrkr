using System;

namespace bstrkr.core.services.location
{
	public interface ILocationService
	{
		event EventHandler<LocationUpdatedEventArgs> LocationUpdated;

		void StartUpdating();
		void StopUpdating();
	}
}