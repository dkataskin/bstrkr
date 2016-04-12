using System;
using System.Globalization;

using Cirrious.CrossCore.Converters;

namespace bstrkr.mvvm.converters
{
    public class RouteNumberToStringConverter : MvxValueConverter<string, string>
    {
        private const string RouteNumberFormatString = "№{0}";

        protected override string Convert(string value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Format(RouteNumberFormatString, value);
        }
    }
}