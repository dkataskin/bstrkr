using System;

namespace bstrkr.core.services.resources
{
	public interface IResourceManager
	{
		object GetVehicleMarker(VehicleTypes type);
	}
}