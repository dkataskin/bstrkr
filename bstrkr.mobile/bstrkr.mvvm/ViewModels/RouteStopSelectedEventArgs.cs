using System;

namespace bstrkr.mvvm.viewmodels
{
    public class RouteStopSelectedEventArgs : EventArgs
    {
        public RouteStopSelectedEventArgs(RouteStopMapViewModel routeStop)
        {
            this.RouteStop = routeStop;
        }

        public RouteStopMapViewModel RouteStop { get; }
    }
}