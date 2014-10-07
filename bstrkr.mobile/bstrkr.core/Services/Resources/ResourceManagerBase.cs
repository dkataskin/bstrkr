using System;

namespace bstrkr.core.services.resources
{
	public abstract class ResourceManagerBase : IResourceManager
	{
		private readonly Lazy<object> _busIcon;
		private readonly Lazy<object> _shuttleIcon;
		private readonly Lazy<object> _trollIcon;

		public ResourceManagerBase()
		{
			_busIcon = new Lazy<object>(() => this.GetImageResource("bus.png"));
			_shuttleIcon = new Lazy<object>(() => this.GetImageResource("shuttle.png"));
			_trollIcon = new Lazy<object>(() => this.GetImageResource("troll.png"));
		}

		public object GetVehicleMarker(VehicleTypes type)
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
	}
}