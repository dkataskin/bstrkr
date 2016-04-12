using System;

namespace bstrkr.mvvm.views
{
    public class MapMarkerClickEventArgs : EventArgs
    {
        public MapMarkerClickEventArgs(IMapMarker mapMarker)
        {
            this.Marker = mapMarker;
        }

        public IMapMarker Marker { get; private set; }
    }
}