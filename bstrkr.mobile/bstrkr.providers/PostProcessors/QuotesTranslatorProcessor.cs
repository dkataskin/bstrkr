using System.Collections.Generic;
using System.Linq;
using System.Text;

using bstrkr.core;

namespace bstrkr.providers.postprocessors
{
    public class QuotesTranslatorProcessor : IRouteStopsDataPostProcessor
    {
        private const char QuoteToReplace = '"';
        private const char LeftQuote = '«';
        private const char RightQuote = '»';

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
            var first = routeStopName.IndexOf(QuoteToReplace);
            var last = routeStopName.LastIndexOf(QuoteToReplace);

            if (first >= 0 && last >= 0)
            {
                var sb = new StringBuilder(routeStopName);
                sb[first] = LeftQuote;
                sb[last] = RightQuote;

                return sb.ToString();
            }

            return routeStopName;
        }
    }
}