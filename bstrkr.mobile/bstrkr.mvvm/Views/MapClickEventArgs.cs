using System;

using bstrkr.core.spatial;

namespace bstrkr.mvvm.views
{
    public class MapClickEventArgs : EventArgs
    {
        public MapClickEventArgs(GeoPoint point)
        {
            this.Point = point;
        }

        public GeoPoint Point { get; private set; }
    }
}