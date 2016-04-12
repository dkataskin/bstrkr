namespace bstrkr.mvvm.navigation
{
    public class RouteStopListItem
    {
        public RouteStopListItem()
        {
        }

        public RouteStopListItem(string routeStopId, string routeStopName, string routeStopDescription)
        {
            this.RouteStopId = routeStopId;
            this.RouteStopName = routeStopName;
            this.RouteStopDescription = routeStopDescription;
        }

        public string RouteStopId { get; set; }

        public string RouteStopName { get; set; }

        public string RouteStopDescription { get; set; }
    }
}