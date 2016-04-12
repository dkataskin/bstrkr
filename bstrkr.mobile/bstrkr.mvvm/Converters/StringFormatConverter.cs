using System;
using System.Globalization;

using Cirrious.CrossCore.Converters;

namespace bstrkr.mvvm.converters
{
    public class StringFormatConverter : IMvxValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
            {
                return value == null ? string.Empty : value.ToString();
            }

            return string.Format(parameter.ToString(), value ?? string.Empty);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}