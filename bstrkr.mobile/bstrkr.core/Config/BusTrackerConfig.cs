using System.Collections.Generic;

namespace bstrkr.core.config
{
    public class BusTrackerConfig
    {
        public List<Area> Areas { get; set; }

        public float ShowRouteStopsZoomThreshold { get; set; }

        public float ShowVehicleTitlesZoomThreshold { get; set; }

        public float AnimateMarkersMovementZoomThreshold { get; set; }
    }
}