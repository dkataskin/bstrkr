using System;
using System.Globalization;

using bstrkr.core;
using bstrkr.core.services.resources;

using Cirrious.CrossCore;
using Cirrious.CrossCore.Converters;

namespace bstrkr.mvvm.converters
{
	public class VehicleTypeToImageConverter : MvxValueConverter<VehicleTypes, string>
	{
		protected override string Convert(VehicleTypes value, Type targetType, object parameter, CultureInfo culture)
		{
			return string.Format("vehicletypes_{0}", value.ToString().ToLower());
		}

		protected override VehicleTypes ConvertBack(string value, Type targetType, object parameter, CultureInfo culture)
		{
			return base.ConvertBack(value, targetType, parameter, culture);
		}
	}
}