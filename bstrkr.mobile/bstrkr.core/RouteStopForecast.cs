using System.Collections.Generic;
using System.Linq;

namespace bstrkr.core
{
    public class RouteStopForecast
    {
        public RouteStopForecast()
        {
            this.Items = new List<RouteStopForecastItem>();
        }

        public RouteStopForecast(string routeStopId, IEnumerable<RouteStopForecastItem> items)
        {
            this.RouteStopId = routeStopId;
            this.Items = items.ToList();
        }

        public string RouteStopId { get; set; }

        public List<RouteStopForecastItem> Items { get; set; }
    }
}