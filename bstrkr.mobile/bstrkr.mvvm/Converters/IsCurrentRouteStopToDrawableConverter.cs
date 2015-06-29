using System;
using System.Globalization;

using Cirrious.CrossCore.Converters;

namespace bstrkr.mvvm.converters
{
	public class IsCurrentRouteStopToDrawableConverter : MvxValueConverter<bool, string>
	{
		protected override string Convert(bool value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value)
			{
				return "track_current";
			}

			return "track";
		}
	}
}