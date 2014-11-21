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
	public class DefaultLiveDataProviderFactory : ILiveDataProviderFactory
	{
		public ILiveDataProvider CreateProvider(Area area)
		{
			if (area == null)
			{
				return null;
			}

			return new Bus13LiveDataProvider(
										area.Endpoint, 
										area.Id,
										TimeSpan.FromMilliseconds(10000));
		}
	}
}