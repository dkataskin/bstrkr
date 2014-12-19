using System;

namespace bstrkr.core.services.location
{
	public class BusTrackerLocationErrorEventArgs : EventArgs
	{
		public BusTrackerLocationErrorEventArgs(BusTrackerLocationErrors error)
		{
			this.Error = error;
		}

		public BusTrackerLocationErrors Error { get; private set; }
	}
}