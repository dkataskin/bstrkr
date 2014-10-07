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

		public LocationUpdatedEventArgs(double latitude, double longitude, double? accuracy = null)
		{
			this.Location = new GeoPoint(latitude, longitude);
			this.Accuracy = accuracy;
		}

		public GeoPoint Location { get; private set; }

		public double? Accuracy { get; private set; }
	}
}