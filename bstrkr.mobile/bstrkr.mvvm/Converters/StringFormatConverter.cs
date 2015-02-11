using System;
using System.Globalization;

using Cirrious.CrossCore.Converters;

namespace bstrkr.mvvm.converters
{
	public class StringFormatConverter : IMvxValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var strValue = value == null ? string.Empty : value.ToString();
			if (parameter == null)
			{
				return strValue;
			}

			return string.Format(parameter.ToString(), strValue);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}