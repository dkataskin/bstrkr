using System;
using System.Globalization;

using bstrkr.core;

using Cirrious.CrossCore.Converters;
using Cirrious.CrossCore.UI;
using Cirrious.CrossCore;

namespace bstrkr.mvvm.converters
{
    public class VehicleTypeToColorConverter : MvxValueConverter<VehicleTypes, object>
    {
        protected override object Convert(VehicleTypes value, Type targetType, object parameter, CultureInfo culture)
        {
            MvxColor color;
            switch (value)
            {
                case VehicleTypes.Bus:
                    color = new MvxColor(0xFF2B9A79);
                    break;

                case VehicleTypes.Trolley:
                    color = new MvxColor(0xFF2C8DDE);
                    break;

                case VehicleTypes.Tram:
                    color = new MvxColor(0xFFF10945);
                    break;

                case VehicleTypes.MiniBus:
                    color = new MvxColor(0xFFF59808);
                    break;

                default:
                    color = new MvxColor(0xFFFFFFFF);
                    break;
            }

            var nativeColorConv = Mvx.Resolve<IMvxNativeColor>();

            return nativeColorConv.ToNative(color);
        }
    }
}