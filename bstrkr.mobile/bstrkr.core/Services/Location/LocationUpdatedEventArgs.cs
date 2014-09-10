using System;

namespace bstrkr.core.services.location
{
	public class LocationUpdatedEventArgs : EventArgs
	{
		public LocationUpdatedEventArgs(float latitude, float longitude)
		{
			this.Latitude = latitude;
			this.Longitude = longitude;
		}

		public float Latitude { get; private set; }

		public float Longitude { get; private set; }
	}
}