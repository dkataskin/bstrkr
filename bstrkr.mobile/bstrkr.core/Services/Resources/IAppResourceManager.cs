using System;

using bstrkr.core.map;

namespace bstrkr.core.services.resources
{
	public interface IAppResourceManager
	{
		object GetVehicleMarker(VehicleTypes type, MapMarkerSizes size, bool isSelected);

		object GetRouteStopMarker(MapMarkerSizes size, bool isSelected);
	}
}