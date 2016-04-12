using System;
using System.Globalization;

using bstrkr.core;

using Cirrious.CrossCore.Converters;

namespace bstrkr.mvvm.converters
{
    public class VehicleTypeToImageConverter : MvxValueConverter<VehicleTypes, string>
    {
        protected override string Convert(VehicleTypes value, Type targetType, object parameter, CultureInfo culture)
        {
            return $"vehicletypes_{value.ToString().ToLower()}";
        }
    }
}