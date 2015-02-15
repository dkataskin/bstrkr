using System;
using System.Collections.Generic;

namespace bstrkr.mvvm.navigation
{
	public class RouteStopListNavParam
	{
		public RouteStopListNavParam()
		{
			this.RouteStops = new List<RouteStopListItem>();
		}

		public List<RouteStopListItem> RouteStops { get; set; }
	}
}