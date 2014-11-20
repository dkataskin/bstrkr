using System;
using System.Linq;

using bstrkr.core;
using bstrkr.core.config;
using bstrkr.core.consts;
using bstrkr.core.services.location;
using bstrkr.core.spatial;
using bstrkr.core.providers.bus13;

namespace bstrkr.providers
{
	public class LiveDataProviderFactory : ILiveDataProviderFactory
	{
		public ILiveDataProvider CreateProvider(Area area)
		{
			return new Bus13LiveDataProvider(
										area.Endpoint, 
										area.Id,
										TimeSpan.FromMilliseconds(10000));
		}
	}
}