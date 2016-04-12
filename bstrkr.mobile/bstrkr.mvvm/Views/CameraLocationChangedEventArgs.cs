using System;

using bstrkr.core.spatial;

namespace bstrkr.mvvm.views
{
    public class CameraLocationChangedEventArgs : EventArgs
    {
        public CameraLocationChangedEventArgs(GeoPoint location, GeoRect projectionBounds)
        {
            this.Location = location;
            this.ProjectionBounds = projectionBounds;
        }

        public GeoPoint Location { get; private set; }

        public GeoRect ProjectionBounds { get; private set; }
    }
}