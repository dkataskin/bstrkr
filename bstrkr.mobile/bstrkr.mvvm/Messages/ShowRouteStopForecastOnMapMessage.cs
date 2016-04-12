using Cirrious.MvvmCross.Plugins.Messenger;

namespace bstrkr.mvvm.messages
{
    public class ShowRouteStopForecastOnMapMessage : MvxMessage
    {
        public ShowRouteStopForecastOnMapMessage(object sender, string routeStopId) : base(sender)
        {
            this.RouteStopId = routeStopId;
        }

        public string RouteStopId { get; private set; }
    }
}