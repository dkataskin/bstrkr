using System;
using System.Collections.Generic;

using bstrkr.core.map;

namespace bstrkr.core.services.resources
{
	public abstract class ResourceManagerBase : IResourceManager
	{
		private const string VehicleMarkerImgFormatString = "{0}_{1}.png";

		private readonly IDictionary<string, object> _cache = new Dictionary<string, object>();

		public object GetVehicleMarker(VehicleTypes type, MapMarkerSizes size)
		{
			var key = string.Format(
						VehicleMarkerImgFormatString, 
						type.ToString().ToLower(), 
						size.ToString().ToLower()[0]);

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

		protected abstract object GetImageResource(string path);

		protected abstract object ScaleImage(object image, float ratio);
	}
}