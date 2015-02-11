using System;
using System.Globalization;

using bstrkr.core;

using Cirrious.CrossCore.Converters;

namespace bstrkr.mvvm.converters
{
	public class ArriveTimeToStringConverter : MvxValueConverter<int, string>
	{
		protected override string Convert(int value, Type targetType, object parameter, CultureInfo culture)
		{
			return string.Format(AppResources.route_vehicle_arrives_arrives_format, value / 60, value % 60);
		}
	}
}