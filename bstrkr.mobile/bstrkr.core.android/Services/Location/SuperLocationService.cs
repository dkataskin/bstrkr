using System;

using Cirrious.MvvmCross.Plugins.Location;

using bstrkr.core.services.location;
using bstrkr.core.spatial;

namespace bstrkr.core.android.services.location
{
	public class SuperLocationService : ILocationService
	{
		private IMvxLocationWatcher _locationWatcher;

		public SuperLocationService(IMvxLocationWatcher locationWatcher)
		{
			_locationWatcher = locationWatcher;
		}

		public event EventHandler<LocationUpdatedEventArgs> LocationUpdated;

		public void StartUpdating()
		{
			_locationWatcher.Start(
						new MvxLocationOptions 
						{ 
							Accuracy = MvxLocationAccuracy.Fine,
							TimeBetweenUpdates = TimeSpan.FromMilliseconds(1000),
						},
						this.OnSuccess,
						this.OnError);
		}

		public void StopUpdating()
		{
			_locationWatcher.Stop();
		}

		private void OnSuccess(MvxGeoLocation geoLocation)
		{
			if (this.LocationUpdated != null)
			{
				this.LocationUpdated(
								this, 
								new LocationUpdatedEventArgs(geoLocation.Coordinates.Latitude,
															 geoLocation.Coordinates.Longitude,
															 geoLocation.Coordinates.Accuracy));
			}
		}

		private void OnError(MvxLocationError locationError)
		{
		}
	}
}