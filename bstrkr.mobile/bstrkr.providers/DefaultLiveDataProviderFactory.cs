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
		private readonly IBusTrackerLocationService _locationService;

		private Area _currentArea;
		private ILiveDataProvider _currentProvider;

		public DefaultLiveDataProviderFactory(IBusTrackerLocationService locationService)
		{
			_locationService = locationService;
			_locationService.LocationChanged += this.OnLocationChanged;

			_currentArea = _locationService.Area;
		}

		public ILiveDataProvider GetCurrentProvider()
		{
			lock(_locationService)
			{
				var area = _locationService.Area;
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

		private void OnLocationChanged(object sender, EventArgs args)
		{
			lock(_locationService)
			{
				var area = _locationService.Area;
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