using System;

namespace bstrkr.core.services.location
{
	public class LocationErrorEventArgs : EventArgs
	{
		public LocationErrorEventArgs(string reason)
		{
			this.Reason = reason;
		}

		public string Reason { get; private set; }
	}
}