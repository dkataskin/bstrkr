using System;
using System.Globalization;

using Cirrious.CrossCore.Converters;
using bstrkr.core;

namespace Converters
{
	public class DistanceToStrConverter : MvxValueConverter<int, string>
	{
		protected override string Convert(int value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value < 10)
			{
				return string.Empty;
			}

			if (value < 1000)
			{
				return string.Format("{0}{1}", value, AppResources.meters_short);
			}

			return string.Format("{0:F2}{1}", value / 1000.0d, AppResources.kilometers_short);
		}
	}
}