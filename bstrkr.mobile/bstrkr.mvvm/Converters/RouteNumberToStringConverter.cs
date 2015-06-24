using System;
using System.Globalization;

using Cirrious.CrossCore.Converters;

namespace Converters
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