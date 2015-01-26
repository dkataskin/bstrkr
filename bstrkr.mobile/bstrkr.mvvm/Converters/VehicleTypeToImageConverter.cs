using System;
using System.Globalization;

using bstrkr.core;
using bstrkr.core.services.resources;

using Cirrious.CrossCore;
using Cirrious.CrossCore.Converters;

namespace bstrkr.mvvm.converters
{
	public class VehicleTypeToImageConverter : IMvxValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return string.Format("vehicletypes_{0}", value.ToString().ToLower());
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}