using System;
using System.Collections.Generic;

using bstrkr.core.map;

namespace bstrkr.core.services.resources
{
	public abstract class ResourceManagerBase : IResourceManager
	{
		private const string VehicleMarkerImgFormatString = "{0}_{1}.png";
		private const string RouteStopMarkerImgFormatString = "busstop_{0}.png";

		private readonly IDictionary<string, object> _cache = new Dictionary<string, object>();

		public object GetVehicleMarker(VehicleTypes type, MapMarkerSizes size)
		{
			var key = string.Format(
						VehicleMarkerImgFormatString, 
						type.ToString().ToLower(), 
						this.GetSizeKey(size));

			return this.GetImageFromCache(key);
		}

		public object GetRouteStopMarker(MapMarkerSizes size)
		{
			var key = string.Format(RouteStopMarkerImgFormatString, this.GetSizeKey(size));

			return this.GetImageFromCache(key);
		}

		private object GetImageFromCache(string key)
		{
			lock(_cache)
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

		private string GetSizeKey(MapMarkerSizes size)
		{
			return size.ToString().ToLower()[0];
		}

		protected abstract object GetImageResource(string name);
	}
}