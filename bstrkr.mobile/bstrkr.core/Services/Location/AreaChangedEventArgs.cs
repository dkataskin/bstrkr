using System;

using bstrkr.core.spatial;

namespace bstrkr.core.services.location
{
    public class AreaChangedEventArgs : EventArgs
    {
        public AreaChangedEventArgs(Area area, bool detected, GeoPoint lastLocation)
        {
            this.Area = area;
            this.Detected = detected;
            this.LastLocation = lastLocation;
        }

        public Area Area { get; private set; }

        public bool Detected { get; private set; }

        public GeoPoint LastLocation { get; set; }
    }
}