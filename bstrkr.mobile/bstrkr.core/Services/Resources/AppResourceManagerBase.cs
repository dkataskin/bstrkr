using System.Collections.Generic;

using bstrkr.core.map;

namespace bstrkr.core.services.resources
{
    public abstract class AppResourceManagerBase : IAppResourceManager
    {
        private const string VehicleMarkerImgFormatString = "{0}_{1}{2}.png";
        private const string RouteStopMarkerImgFormatString = "busstop_{0}{1}.png";
        private const string SelectedMarkerDiscriminator = "_selected";

        private readonly IDictionary<string, object> _cache = new Dictionary<string, object>();

        public object GetVehicleMarker(VehicleTypes type, MapMarkerSizes size, bool isSelected)
        {
            var key = string.Format(
                        VehicleMarkerImgFormatString,
                        type.ToString().ToLower(),
                        this.GetSizeKey(size),
                        this.GetSelectedKey(isSelected));

            return this.GetImageFromCache(key);
        }

        public object GetRouteStopMarker(MapMarkerSizes size, bool isSelected)
        {
            var key = string.Format(
                        RouteStopMarkerImgFormatString,
                        this.GetSizeKey(size),
                        this.GetSelectedKey(isSelected));

            return this.GetImageFromCache(key);
        }

        public abstract object GetVehicleTitleMarker(VehicleTypes type, string title);

        private object GetImageFromCache(string key)
        {
            lock (_cache)
            {
                if (_cache.ContainsKey(key))
                {
                    return _cache[key];
                }

                var image = this.GetImageResource(key);
                _cache[key] = image;

                return image;
            }
        }

        private char GetSizeKey(MapMarkerSizes size)
        {
            return size.ToString().ToLower()[0];
        }

        private string GetSelectedKey(bool isSelected)
        {
            return isSelected ? SelectedMarkerDiscriminator : string.Empty;
        }

        protected abstract object GetImageResource(string name);
    }
}