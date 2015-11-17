using System;
using System.Collections.Generic;

namespace bstrkr.providers.postprocessors
{
	public class PostProcessorFactory
	{
		private const string RuCultureISOName = "rus";

		public PostProcessorFactory()
		{
		}

		public IEnumerable<IRouteStopsDataPostProcessor> CreateRouteStopsDataPostProcessors(DataServiceContext context)
		{
			if (context.CurrentUIThreeLetterISOName.Equals(RuCultureISOName))
			{
				return new[] { new RouteStopNameTranslatorProcessor() };
			}

			return new List<IRouteStopsDataPostProcessor>();
		}
	}
}