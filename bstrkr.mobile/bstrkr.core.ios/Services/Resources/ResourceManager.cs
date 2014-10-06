using System;

using bstrkr.core.services.resources;
using MonoTouch.UIKit;

namespace bstrkr.core.ios.services.resources
{
	public class ResourceManager : IResourceManager
	{
		public object GetVehicleMarker(VehicleTypes type)
		{
			switch (type)
			{
				case VehicleTypes.Bus:
					return UIImage.FromFile("bus.png");
					break;

				case VehicleTypes.ShuttleBus:
					return UIImage.FromFile("shuttle.png");
					break;

				case VehicleTypes.Trolleybus:
					return UIImage.FromFile("troll.png");
					break;

				default:
					return UIImage.FromFile("bus.png");
					break;
			}
		}
	}
}