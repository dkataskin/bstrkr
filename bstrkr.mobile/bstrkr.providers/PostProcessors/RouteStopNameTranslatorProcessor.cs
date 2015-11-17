using System;
using System.Collections.Generic;
using System.Linq;

using bstrkr.core;

namespace bstrkr.providers.postprocessors
{
	public class RouteStopNameTranslatorProcessor : IRouteStopsDataPostProcessor
	{
		private const string QuoteToReplace = "\"";
		private const string Quote = "«";

		public IEnumerable<RouteStop> Process(IEnumerable<RouteStop> stops)
		{
			return stops.Select(stop => 
							new RouteStop(
										stop.Id,
										this.ConvertQuotes(stop.Name),
										stop.Description,
										stop.Location.Position)).ToList();
		}

		private string ConvertQuotes(string routeStopName)
		{
			return routeStopName.Replace(QuoteToReplace, Quote);
		}
	}
}