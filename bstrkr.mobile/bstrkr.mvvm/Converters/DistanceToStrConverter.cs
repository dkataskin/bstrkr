using System;
using System.Globalization;

using Cirrious.CrossCore.Converters;

using bstrkr.core;

namespace bstrkr.mvvm.converters
{
    public class DistanceToStrConverter : MvxValueConverter<int, string>
    {
        protected override string Convert(int value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value < 10)
            {
                return string.Empty;
            }

            if (value < 1000)
            {
                return $"{value}{AppResources.meters_short}";
            }

            return $"{value/1000.0d:F2}{AppResources.kilometers_short}";
        }
    }
}