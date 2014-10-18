using System;
using System.Collections.Generic;

using bstrkr.core.map;

namespace bstrkr.core.services.resources
{
	public abstract class ResourceManagerBase : IResourceManager
	{
		private const string VehicleMarkerImgFormatString = "{0}_{1}.png";

		private readonly IDictionary<int, object> _cache = new Dictionary<int, object>();

		private readonly Lazy<object> _busIcon;
		private readonly Lazy<object> _shuttleIcon;
		private readonly Lazy<object> _trollIcon;

		public ResourceManagerBase()
		{
			_busIcon = new Lazy<object>(() => this.GetImageResource("bus.png"));
			_shuttleIcon = new Lazy<object>(() => this.GetImageResource("shuttle.png"));
			_trollIcon = new Lazy<object>(() => this.GetImageResource("troll.png"));
		}

		public object GetVehicleMarker(VehicleTypes type, MapMarkerSizes size)
		{
			var key = string.Format(VehicleMarkerImgFormatString, type, size);

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