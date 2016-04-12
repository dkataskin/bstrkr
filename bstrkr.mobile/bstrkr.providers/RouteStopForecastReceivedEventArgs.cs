using System;

using bstrkr.core;

namespace bstrkr.providers
{
    public class RouteStopForecastReceivedEventArgs : EventArgs
    {
        public RouteStopForecastReceivedEventArgs(string routeStopId, RouteStopForecast forecast)
        {
            this.RouteStopId = routeStopId;
            this.Forecast = forecast;
        }

        public string RouteStopId { get; private set; }

        public RouteStopForecast Forecast { get; private set; }
    }
}