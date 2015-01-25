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
		private IAppResourceManager _resourceManager;

		public VehicleTypeToImageConverter()
		{
			_resourceManager = Mvx.Resolve<IAppResourceManager>();
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return _resourceManager.GetVehicleTypeIcon((VehicleTypes)value);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return _resourceManager.GetVehicleTypeIcon((VehicleTypes)value);
		}
	}
}