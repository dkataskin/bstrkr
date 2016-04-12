using System;
using System.Globalization;

using bstrkr.core;
using Cirrious.CrossCore.Converters;

namespace bstrkr.mvvm.converters
{
    public class VehicleTypeToStringConverter : MvxValueConverter<VehicleTypes, string>
    {
        protected override string Convert(VehicleTypes value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case VehicleTypes.Bus:
                    return AppResources.bus_vehicle_type_title;

                case VehicleTypes.MiniBus:
                    return AppResources.minibus_vehicle_type_title;

                case VehicleTypes.Trolley:
                    return AppResources.troll_vehicle_type_title;

                case VehicleTypes.Tram:
                    return AppResources.tramway_vehicle_type_title;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}