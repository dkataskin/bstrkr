using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using Cirrious.CrossCore.Converters;

using bstrkr.mvvm.views;

namespace bstrkr.mvvm.converters
{
	public class ZoomToMarkerSizeConverter : IMvxValueConverter
	{
		private readonly IList<Tuple<float, MapMarkerSizes>> _map = new List<Tuple<float, MapMarkerSizes>>
		{
			new Tuple<float, MapMarkerSizes>(16.0f, MapMarkerSizes.Big),
			new Tuple<float, MapMarkerSizes>(12.0f, MapMarkerSizes.BigMedium),
			new Tuple<float, MapMarkerSizes>(8.0f, MapMarkerSizes.Medium),
			new Tuple<float, MapMarkerSizes>(4.0f, MapMarkerSizes.Small)
		};

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (targetType != typeof(MapMarkerSizes))
			{
				throw new InvalidOperationException("Invalid target type");
			}

			var zoom = (float)value;
			if (zoom >= _map.First().Item1)
			{
				return _map.First().Item2;
			}

			foreach (var tuple in _map)
			{
				if (zoom <= tuple.Item1)
				{
					return tuple.Item2;
				}
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}