using System;

using System.Linq;

using bstrkr.core.config;
using bstrkr.core.consts;
using bstrkr.core.spatial;

namespace bstrkr.core.services.location
{
	public class BusTrackerLocationService : IBusTrackerLocationService
	{
		private readonly ILocationService _locationService;
		private readonly IConfigManager _configManager;

		private bool _unknownAreaReported;

		public BusTrackerLocationService(IConfigManager configManager, ILocationService locationService)
		{
			_configManager = configManager;

			_locationService = locationService;
			_locationService.LocationUpdated += this.OnLocationUpdated;
		}

		public event EventHandler<EventArgs> LocationChanged;

		public event EventHandler<LocationErrorEventArgs> LocationError;

		public Area Area { get; private set; }

		public GeoPoint Location { get; private set; }

		public double? Accuracy { get; private set; }

		public void Start()
		{
			_locationService.StartUpdating();
		}

		private void OnLocationUpdated(object sender, LocationUpdatedEventArgs args)
		{
			this.Location = args.Location;
			this.Accuracy = args.Accuracy;

			if (this.Area == null)
			{
				var config = _configManager.GetConfig();

				var location = config.Areas
									 .Select(x => new Tuple<double, Area>(
															this.Location.DistanceTo(new GeoPoint(x.Latitude, x.Longitude)), 
															x))
									 .OrderBy(x => x.Item1)
								     .First();

				if (location.Item1 <= AppConsts.MaxDistanceFromCityCenter)
				{
					this.Area = location.Item2;
				}
				else if (!_unknownAreaReported)
				{
					this.RaiseLocationErrorEvent(LocationErrors.UnknownArea);
					_unknownAreaReported = true;
				}
			}

			this.RaiseLocationChangedEvent();
		}

		private void RaiseLocationChangedEvent()
		{
			if (this.LocationChanged != null)
			{
				this.LocationChanged(this, EventArgs.Empty);
			}
		}

		private void RaiseLocationErrorEvent(LocationErrors error)
		{
			if (this.LocationError != null)
			{
				this.LocationError(this, new LocationErrorEventArgs(error));
			}
		}
	}
}