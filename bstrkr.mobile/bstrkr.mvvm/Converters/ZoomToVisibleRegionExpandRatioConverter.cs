using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using bstrkr.core.map;
using bstrkr.mvvm.views;

using Cirrious.CrossCore.Converters;

namespace bstrkr.mvvm.converters
{
	public class ZoomToVisibleRegionExpandRatioConverter : MvxValueConverter<float, float>
	{
		private readonly IList<Tuple<float, float>> _map = new List<Tuple<float, float>>
		{
			new Tuple<float, float>(14.0f, 1.2f),
			new Tuple<float, float>(15.0f, 1.5f),
			new Tuple<float, float>(16.0f, 2.0f),
			new Tuple<float, float>(17.0f, 3.0f)
		};

		protected override float Convert(float value, Type targetType, object parameter, CultureInfo culture)
		{
			if (targetType != typeof(float))
			{
				throw new InvalidOperationException("Invalid target type");
			}

			var zoom = (float)value;

			if (zoom < _map.First().Item1)
			{
				return 1.0f;
			}

			var item = _map.FirstOrDefault(x => zoom <= x.Item1);
			if (item != null)
			{
				return item.Item2;
			}

			return 3.0f;
		}
	}
}