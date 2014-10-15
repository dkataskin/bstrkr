using System;
using System.Collections.Generic;

namespace bstrkr.core.services.resources
{
	public abstract class ResourceManagerBase : IResourceManager
	{
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

		public object GetVehicleMarker(VehicleTypes type, double mapZoom)
		{
			object image = null;

			switch (type)
			{
				case VehicleTypes.Bus:
					image = _busIcon.Value;
					break;

				case VehicleTypes.ShuttleBus:
					image = _shuttleIcon.Value;
					break;

				case VehicleTypes.Trolleybus:
					image = _trollIcon.Value;
					break;

				default:
					image = _busIcon.Value;
					break;
			}

			var key = (Convert.ToInt32(type) << 32) ^ Convert.ToInt32(mapZoom);

			lock(_cache)
			{
				if (_cache.ContainsKey(key))
				{
					return _cache[key];
				}

				if (mapZoom >= 16.0d)
				{
					return image;
				}

				if (mapZoom >= 14.0d)
				{
					_cache[key] = this.ScaleImage(image, 0.5f);
					return _cache[key];
				}

				if (mapZoom >= 12.0d)
				{
					_cache[key] = this.ScaleImage(image, 0.4f);
					return _cache[key];
				}

				if (mapZoom >= 10.0d)
				{
					_cache[key] = this.ScaleImage(image, 0.3f);
					return _cache[key];
				}

				if (mapZoom >= 8.0d)
				{
					_cache[key] = this.ScaleImage(image, 0.2f);
					return _cache[key];
				}

				if (mapZoom >= 6.0d)
				{
					_cache[key] = this.ScaleImage(image, 0.1f);
					return _cache[key];
				}

				if (mapZoom >= 4.0d)
				{
					_cache[key] = this.ScaleImage(image, 0.05f);
					return _cache[key];
				}

				return this.ScaleImage(image, 0.01f);
			}
		}

		protected abstract object GetImageResource(string path);

		protected abstract object ScaleImage(object image, float ratio);
	}
}