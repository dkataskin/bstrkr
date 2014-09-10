using System;

namespace bstrkr.core.services.location
{
	public class LocationUpdatedEventArgs : EventArgs
	{
		public LocationUpdatedEventArgs(double latitude, double longitude)
		{
			this.Latitude = latitude;
			this.Longitude = longitude;
		}

		public double Latitude { get; private set; }

		public double Longitude { get; private set; }
	}
}