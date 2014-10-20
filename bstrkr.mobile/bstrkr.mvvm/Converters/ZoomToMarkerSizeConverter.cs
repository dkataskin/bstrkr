using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using Cirrious.CrossCore.Converters;

using bstrkr.core.map;
using bstrkr.mvvm.views;

namespace bstrkr.mvvm.converters
{
	public class ZoomToMarkerSizeConverter : IMvxValueConverter
	{
		private readonly IList<Tuple<float, MapMarkerSizes>> _map = new List<Tuple<float, MapMarkerSizes>>
		{
			new Tuple<float, MapMarkerSizes>(19.0f, MapMarkerSizes.Huge),
			new Tuple<float, MapMarkerSizes>(17.0f, MapMarkerSizes.Big),
			new Tuple<float, MapMarkerSizes>(15.0f, MapMarkerSizes.Medium),
			new Tuple<float, MapMarkerSizes>(12.0f, MapMarkerSizes.Small)
		};

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return this.ConvertBack(value, targetType, parameter, culture);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (targetType != typeof(MapMarkerSizes))
			{
				throw new InvalidOperationException("Invalid target type");
			}

			var zoom = (float)value;
			foreach (var tuple in _map)
			{
				if (zoom >= tuple.Item1)
				{
					return tuple.Item2;
				}
			}

			return MapMarkerSizes.Tiny;
		}
	}
}