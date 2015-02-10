using System;
using System.Globalization;

using bstrkr.core;

using Cirrious.CrossCore.Converters;

namespace bstrkr.mvvm.converters
{
	public class VehicleCarPlateToTitleConverter : MvxValueConverter<string, string>
	{
		protected override string Convert(string value, Type targetType, object parameter, CultureInfo culture)
		{
			return string.Format(AppResources.route_vehicle_format, value);
		}
	}
}