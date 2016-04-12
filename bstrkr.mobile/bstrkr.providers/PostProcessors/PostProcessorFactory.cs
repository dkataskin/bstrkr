using System;
using System.Collections.Generic;

namespace bstrkr.providers.postprocessors
{
    public class PostProcessorFactory
    {
        private const string RuCultureISOName = "ru";

        public IEnumerable<IRouteStopsDataPostProcessor> CreateRouteStopsDataPostProcessors(DataServiceContext context)
        {
            if (string.Equals(context.CurrentUIThreeLetterISOName, RuCultureISOName, StringComparison.OrdinalIgnoreCase))
            {
                return new[] { new QuotesTranslatorProcessor() };
            }

            return new List<IRouteStopsDataPostProcessor>();
        }
    }
}