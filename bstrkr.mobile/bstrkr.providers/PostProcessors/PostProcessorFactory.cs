using System;
using System.Collections.Generic;

namespace bstrkr.providers.postprocessors
{
	public class PostProcessorFactory
	{
		private const string RuCultureISOName = "ru";

		public PostProcessorFactory()
		{
		}

		public IEnumerable<IRouteStopsDataPostProcessor> CreateRouteStopsDataPostProcessors(DataServiceContext context)
		{
			if (string.Equals(context.CurrentUIThreeLetterISOName, RuCultureISOName, StringComparison.OrdinalIgnoreCase))
			{
				return new[] { new RouteStopNameTranslatorProcessor() };
			}

			return new List<IRouteStopsDataPostProcessor>();
		}
	}
}