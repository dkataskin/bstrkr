using System;

using bstrkr.core.map;

namespace bstrkr.core.services.resources
{
	public interface IResourceManager
	{
		object GetVehicleMarker(VehicleTypes type, MapMarkerSizes size);

		object GetRouteStopMarker(MapMarkerSizes size);
	}
}