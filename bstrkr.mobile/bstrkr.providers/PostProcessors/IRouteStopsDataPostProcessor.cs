using System;
using System.Collections.Generic;
using bstrkr.core;

namespace bstrkr.providers.postprocessors
{
	public interface IRouteStopsDataPostProcessor
	{
		IEnumerable<RouteStop> Process(IEnumerable<RouteStop> stops);
	}
}