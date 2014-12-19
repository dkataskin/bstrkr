using System;

namespace bstrkr.core.services.location
{
	public interface ILocationService
	{
		event EventHandler<LocationUpdatedEventArgs> LocationUpdated;

		event EventHandler<LocationErrorEventArgs> LocatingFailed;

		void StartUpdating();

		void StopUpdating();
	}
}