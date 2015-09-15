using System;

using bstrkr.core;
using bstrkr.core.services.location;
using bstrkr.core.spatial;

namespace Services.Location
{
	public class PositioningService : IPositioningService
	{
		private readonly ILocationService _locationService;
		private readonly IAreaPositioningService _areaPositioningService;

		public PositioningService(ILocationService locationService, IAreaPositioningService areaPositioningService)
		{
			_locationService = locationService;
			_areaPositioningService = areaPositioningService;

			_areaPositioningService.AreaLocated += (s, a) => this.RaiseAreaChangedEvent(
																			this.CurrentArea, 
																			this.DetectedArea,
																			_locationService.GetLastLocation());
		}

		public event EventHandler<AreaChangedEventArgs> AreaChanged;

		public Area CurrentArea { get { return _areaPositioningService.Area; } }

		public bool DetectedArea { get { return _areaPositioningService.DetectedArea; } }

		public GeoPoint GetLastLocation()
		{
			if (this.CurrentArea == null)
			{
				return GeoPoint.Empty;
			}

			if (DetectedArea)
			{
				return _locationService.GetLastLocation();
			}

			return new GeoPoint(this.CurrentArea.Latitude, this.CurrentArea.Longitude);
		}

		public void Start()
		{
			_areaPositioningService.Start();
		}

		public void Stop()
		{
			_areaPositioningService.Stop();
		}

		private void RaiseAreaChangedEvent(Area area, bool detected, GeoPoint lastLocation)
		{
			if (this.AreaChanged != null)
			{
				this.AreaChanged(this, new AreaChangedEventArgs(area, detected, lastLocation));
			}
		}
	}
}