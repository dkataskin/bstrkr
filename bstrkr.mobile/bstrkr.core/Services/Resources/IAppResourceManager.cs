using System;

using bstrkr.core.map;

namespace bstrkr.core.services.resources
{
	public interface IAppResourceManager
	{
		object GetVehicleMarker(VehicleTypes type, MapMarkerSizes size);

		object GetRouteStopMarker(MapMarkerSizes size);

		object GetVehicleTypeIcon(VehicleTypes vehicleType);
	}
}