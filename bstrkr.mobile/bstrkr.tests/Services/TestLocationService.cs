using System;
using System.Timers;

using bstrkr.core.services.location;
using bstrkr.core.spatial;

namespace bstrkr.tests.services
{
	public class TestLocationService : ILocationService
	{
		private const double LatitudeVariation = 0.01;
		private const double LongitudeVariation = 0.01;

		private readonly GeoPoint _location;
		private readonly Lazy<Random> _rnd = new Lazy<Random>();

		private Timer _timer;

		public TestLocationService(GeoPoint location)
		{
			_timer = new Timer();
			_timer.Interval = 1000;
			_timer.Elapsed += this.OnTimerElapsed;
		}

		public event EventHandler<LocationUpdatedEventArgs> LocationUpdated;

		public void StartUpdating()
		{
			_timer.Start();
		}

		public void StopUpdating()
		{
			_timer.Stop();
		}

		private void OnTimerElapsed(object sender, ElapsedEventArgs args)
		{
			var lat = this.Variate(_location.Latitude, LatitudeVariation);
			var lon = this.Variate(_location.Longitude, LongitudeVariation);

			this.RaiseLocationUpdatedEvent(new GeoPoint(lat, lon));
		}

		private double Variate(double value, double variation)
		{
			return value + variation * (_rnd.Value.NextDouble() - 0.5);
		}

		private void RaiseLocationUpdatedEvent(GeoPoint location)
		{
			if (this.LocationUpdated != null)
			{
				this.LocationUpdated(this, new LocationUpdatedEventArgs(location));
			}
		}
	}
}