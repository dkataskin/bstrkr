using System;

using bstrkr.core.spatial;

namespace bstrkr.core.services.location
{
	public class LocationUpdatedEventArgs : EventArgs
	{
		public LocationUpdatedEventArgs(GeoPoint location)
		{
			this.Location = location;
		}

		public LocationUpdatedEventArgs(double latitude, double longitude)
		{
			this.Location = new GeoPoint(latitude, longitude);
		}

		public GeoPoint Location { get; private set; }
	}
}