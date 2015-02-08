using System;

using bstrkr.core;

using Cirrious.CrossCore.Converters;

namespace bstrkr.mvvm.converters
{
	public class RouteNumberToTitleConverter
	{
		public string Convert(int routeNumber, VehicleTypes vehicleType)
		{
			switch (vehicleType)
			{
				case VehicleTypes.Bus:
					return string.Format(AppResources.bus_route_title_format, routeNumber);

				case VehicleTypes.MiniBus:
					return string.Format(AppResources.minibus_route_title_format, routeNumber);

				case VehicleTypes.Trolley:
					return string.Format(AppResources.troll_route_title_format, routeNumber);

				case VehicleTypes.Tram:
					return string.Format(AppResources.tramway_route_title_format, routeNumber);

				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}

