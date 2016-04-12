using System;
using System.Globalization;

using bstrkr.core;

using Cirrious.CrossCore.Converters;

namespace bstrkr.mvvm.converters
{
    public class RouteToFirstLastStopsConverter : MvxValueConverter<Route, string>
    {
        private const string FirstLastStopFormatString = "{0} — {1}";

        protected override string Convert(Route value, Type targetType, object parameter, CultureInfo culture)
        {
            var firstStop = value.FirstStop == null ? string.Empty : value.FirstStop.Name;
            var lastStop = value.LastStop == null ? string.Empty : value.LastStop.Name;

            if (string.IsNullOrEmpty(firstStop) && string.IsNullOrEmpty(lastStop))
            {
                return string.Empty;
            }

            if (!string.IsNullOrEmpty(firstStop) && !string.IsNullOrEmpty(lastStop))
            {
                return string.Format(FirstLastStopFormatString, firstStop, lastStop);
            }

            if (!string.IsNullOrEmpty(firstStop))
            {
                return firstStop;
            }

            if (!string.IsNullOrEmpty(lastStop))
            {
                return lastStop;
            }

            return string.Empty;
        }
    }
}