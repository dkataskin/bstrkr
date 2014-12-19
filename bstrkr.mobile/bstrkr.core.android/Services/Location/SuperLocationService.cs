using System;

using Cirrious.MvvmCross.Plugins.Location;

using bstrkr.core.services.location;
using bstrkr.core.spatial;
using Xamarin;

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

		public event EventHandler<LocationErrorEventArgs> LocatingFailed;

		public void StartUpdating()
		{
			_locationWatcher.Start(
						new MvxLocationOptions 
						{ 
							Accuracy = MvxLocationAccuracy.Coarse,
							TimeBetweenUpdates = TimeSpan.FromMilliseconds(1000),
							MovementThresholdInM = 30
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
			switch (locationError.Code)
			{
				case MvxLocationErrorCode.PermissionDenied:
					Insights.Report(null, "location_error", "permission_denied", ReportSeverity.Warning);
					this.RaiseLocatingFailedEvent(LocationErrors.PermissionDenied);
					break;

				case MvxLocationErrorCode.ServiceUnavailable:
					Insights.Report(null, "location_error", "service_unavailable", ReportSeverity.Warning);
					this.RaiseLocatingFailedEvent(LocationErrors.LocationServiceUnavailable);
					break;

				case MvxLocationErrorCode.PositionUnavailable:
					Insights.Report(null, "location_error", "position_unavailable", ReportSeverity.Warning);
					this.RaiseLocatingFailedEvent(LocationErrors.PositionUnavailable);
					break;

				default:
					break;
			}
		}

		private void RaiseLocatingFailedEvent(LocationErrors error)
		{
			if (this.LocatingFailed != null)
			{
				this.LocatingFailed(this, new LocationErrorEventArgs(error));
			}
		}
	}
}