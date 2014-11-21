using System;

namespace bstrkr.core.services.location
{
	public class LocationErrorEventArgs : EventArgs
	{
		public LocationErrorEventArgs(LocationErrors error)
		{
			this.Error = error;
		}

		public LocationErrors Error { get; private set; }
	}
}