using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using Cirrious.CrossCore.Converters;

using bstrkr.core.map;
using bstrkr.mvvm.views;

namespace bstrkr.mvvm.converters
{
	public class ZoomToMarkerSizeConverter : MvxValueConverter<float, MapMarkerSizes>
	{
		private readonly IList<Tuple<float, MapMarkerSizes>> _map = new List<Tuple<float, MapMarkerSizes>>
		{
			new Tuple<float, MapMarkerSizes>(13.0f, MapMarkerSizes.Big),
			new Tuple<float, MapMarkerSizes>(12.0f, MapMarkerSizes.Medium)
		};

		protected override MapMarkerSizes Convert(float value, Type targetType, object parameter, CultureInfo culture)
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

			return MapMarkerSizes.Small;
		}

//		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
//		{
//			if (targetType != typeof(MapMarkerSizes))
//			{
//				throw new InvalidOperationException("Invalid target type");
//			}
//
//			var zoom = (float)value;
//			foreach (var tuple in _map)
//			{
//				if (zoom >= tuple.Item1)
//				{
//					return tuple.Item2;
//				}
//			}
//
//			return MapMarkerSizes.Small;
//		}
//
//		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
//		{
//			if (targetType != typeof(MapMarkerSizes))
//			{
//				throw new InvalidOperationException("Invalid target type");
//			}
//
//			var zoom = (float)value;
//			foreach (var tuple in _map)
//			{
//				if (zoom >= tuple.Item1)
//				{
//					return tuple.Item2;
//				}
//			}
//
//			return MapMarkerSizes.Small;
//		}
	}
}