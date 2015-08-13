using System;

using Cirrious.MvvmCross.Plugins.Messenger;

namespace bstrkr.mvvm.messages
{
	public class ShowRouteStopForecastOnMapMessage : MvxMessage
	{
		public ShowRouteStopForecastOnMapMessage(object sender, string RouteStopId) : base(sender)
		{
			this.RouteStopId = RouteStopId;
		}

		public string RouteStopId { get; private set; }
	}
}