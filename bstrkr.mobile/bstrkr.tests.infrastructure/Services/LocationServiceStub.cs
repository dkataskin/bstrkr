using System;
using System.Threading;
using System.Threading.Tasks;

using bstrkr.core.services.location;
using bstrkr.core.spatial;

namespace bstrkr.tests.infrastructure.services
{
	public class LocationServiceStub : ILocationService
	{
		private const double LatitudeVariation = 0.01;
		private const double LongitudeVariation = 0.01;

		private readonly GeoPoint _location;
		private readonly Lazy<Random> _rnd = new Lazy<Random>();

		private CancellationTokenSource _tokenSource;
		private Task _updateTask;

		public LocationServiceStub(GeoPoint location)
		{
			_location = location;
		}

		public event EventHandler<LocationUpdatedEventArgs> LocationUpdated;

		public void StartUpdating()
		{
			_tokenSource = new CancellationTokenSource();
			_updateTask = Task.Factory.StartNew(
				() => this.UpdateInLoop(TimeSpan.FromMilliseconds(500), _tokenSource.Token), 
				_tokenSource.Token);
		}

		public void StopUpdating()
		{
			_tokenSource.Cancel();
			_updateTask.Wait();

			_tokenSource = null;
			_updateTask = null;
		}

		private void UpdateInLoop(TimeSpan delayInterval, CancellationToken token)
		{
			try
			{
				while(!token.IsCancellationRequested)
				{
					Task.Delay(Convert.ToInt32(delayInterval.TotalMilliseconds)).Wait(token);

					this.UpdateLocation();
				}
			}
			catch (OperationCanceledException e)
			{
			}
		}

		private void UpdateLocation()
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