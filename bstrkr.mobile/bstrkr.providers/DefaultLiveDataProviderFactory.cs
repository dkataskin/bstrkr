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
		private readonly IAreaPositioningService _areaPositioningService;

		private Area _currentArea;
		private ILiveDataProvider _currentProvider;

		public DefaultLiveDataProviderFactory(IAreaPositioningService areaPositioningService)
		{
			_areaPositioningService = areaPositioningService;
			_areaPositioningService.AreaLocated += this.OnAreaChanged;

			_currentArea = _areaPositioningService.Area;
		}

		public ILiveDataProvider GetCurrentProvider()
		{
			lock(_areaPositioningService)
			{
				var area = _areaPositioningService.Area;
				if (_currentArea == null || area == null || !_currentArea.Id.Equals(area.Id) || _currentProvider == null)
				{
					_currentArea = area;
					_currentProvider = this.CreateProvider(_currentArea);

					return _currentProvider;
				}
				else
				{
					return _currentProvider;
				}
			}
		}

		public ILiveDataProvider CreateProvider(Area area)
		{
			if (area == null)
			{
				return null;
			}

			return new Bus13LiveDataProvider(
										area,
										TimeSpan.FromMilliseconds(10000));
		}

		private void OnAreaChanged(object sender, EventArgs args)
		{
			lock(_areaPositioningService)
			{
				var area = _areaPositioningService.Area;
				if (_currentArea == null || area == null)
				{
					_currentArea = area;
					_currentProvider = this.CreateProvider(_currentArea);
				}
				else
				{
					if (!_currentArea.Id.Equals(area.Id))
					{
						_currentArea = area;
						_currentProvider = this.CreateProvider(_currentArea);
					}
				}
			}
		}
	}
}