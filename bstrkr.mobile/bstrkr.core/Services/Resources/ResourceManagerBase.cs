using System;
using System.Collections.Generic;

namespace bstrkr.core.services.resources
{
	public abstract class ResourceManagerBase : IResourceManager
	{
		private readonly IDictionary<int, float> _coefficients = new Dictionary<int, float>();

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
			switch (type)
			{
				case VehicleTypes.Bus:
					return _busIcon.Value;
					break;

				case VehicleTypes.ShuttleBus:
					return _shuttleIcon.Value;
					break;

				case VehicleTypes.Trolleybus:
					return _trollIcon.Value;
					break;

				default:
					return _busIcon.Value;
					break;
			}
		}

		protected abstract object GetImageResource(string path);

		protected abstract object ScaleImage(object image, float ratio);
	}
}