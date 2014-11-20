using System;

using bstrkr.core;
using bstrkr.core.config;
using bstrkr.core.spatial;

namespace bstrkr.providers
{
	public interface ILiveDataProviderFactory
	{
		ILiveDataProvider CreateProvider(Area area);
	}
}