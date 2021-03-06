﻿using System;

using System.Linq;

using bstrkr.core.config;
using bstrkr.core.consts;
using bstrkr.core.spatial;

namespace bstrkr.core.services.location
{
    public class AreaPositioningService : IAreaPositioningService
    {
        private readonly ILocationService _locationService;
        private readonly IConfigManager _configManager;

        private bool _unknownAreaReported;

        public AreaPositioningService(IConfigManager configManager, ILocationService locationService)
        {
            _configManager = configManager;

            _locationService = locationService;
            _locationService.LocationUpdated += this.OnLocationUpdated;
        }

        public event EventHandler<EventArgs> AreaLocated;

        public event EventHandler<EventArgs> AreaLocatingFailed;

        public Area Area { get; private set; }

        public bool DetectedArea { get; private set; }

        public void Start()
        {
            _locationService.StartUpdating();
        }

        public void Stop()
        {
            _locationService.StopUpdating();
        }

        public void SelectArea(Area area)
        {
            _locationService.StopUpdating();

            this.Area = area;
            if (area != null)
            {
                this.DetectedArea = false;
            }

            this.RaiseLocationChangedEvent();
        }

        private void OnLocationUpdated(object sender, LocationUpdatedEventArgs args)
        {
            if (args.Location.Equals(GeoPoint.Empty))
            {
                return;
            }

            if (this.Area == null)
            {
                var config = _configManager.GetConfig();

                var location = config.Areas
                                     .Select(x => new Tuple<double, Area>(
                                                                    args.Location.DistanceTo(new GeoPoint(x.Latitude, x.Longitude)),
                                                                    x))
                                     .OrderBy(x => x.Item1)
                                     .First();

                if (location.Item1 <= AppConsts.MaxDistanceFromCityCenter)
                {
                    this.Area = location.Item2;
                    this.DetectedArea = true;
                }
                else if (!_unknownAreaReported)
                {
                    this.RaiseAreaLocatingFailedEvent();
                    _unknownAreaReported = true;
                }
            }

            this.RaiseLocationChangedEvent();
        }

        private void RaiseLocationChangedEvent()
        {
            this.AreaLocated?.Invoke(this, EventArgs.Empty);
        }

        private void RaiseAreaLocatingFailedEvent()
        {
            this.AreaLocatingFailed?.Invoke(this, EventArgs.Empty);
        }
    }
}