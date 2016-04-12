using System;

using bstrkr.core;
using bstrkr.core.services.location;
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
            _locationService.AreaChanged += this.OnAreaChanged;

            _currentArea = _locationService.CurrentArea;
        }

        public ILiveDataProvider GetCurrentProvider()
        {
            lock (_locationService)
            {
                var area = _locationService.CurrentArea;
                if (_currentArea == null || area == null || !_currentArea.Id.Equals(area.Id) || _currentProvider == null)
                {
                    _currentArea = area;
                    _currentProvider = this.CreateProvider(_currentArea);

                    return _currentProvider;
                }

                return _currentProvider;
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

        private void OnAreaChanged(object sender, AreaChangedEventArgs args)
        {
            lock (_locationService)
            {
                if (_currentArea == null || args.Area == null)
                {
                    _currentArea = args.Area;
                    _currentProvider = this.CreateProvider(_currentArea);
                }
                else
                {
                    if (!_currentArea.Id.Equals(args.Area.Id))
                    {
                        _currentArea = args.Area;
                        _currentProvider = this.CreateProvider(_currentArea);
                    }
                }
            }
        }
    }
}