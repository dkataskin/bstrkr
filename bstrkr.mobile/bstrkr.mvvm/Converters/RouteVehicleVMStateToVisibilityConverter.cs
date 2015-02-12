using System;
using System.Collections.Generic;
using System.Globalization;

using bstrkr.mvvm.viewmodels;

using Cirrious.CrossCore.Converters;
using Cirrious.CrossCore.UI;
using Cirrious.MvvmCross.Plugins.Visibility;

namespace bstrkr.mvvm.converters 
{
	public class RouteVehicleVMStateToVisibilityConverter : MvxBaseVisibilityValueConverter<RouteVehicleVMStates>
	{
		private enum UIParts
		{
			Progress,
			Station,
			Time,
			NoDataText
		}

		private IDictionary<RouteVehicleVMStates, IList<UIParts>> _map;

		public RouteVehicleVMStateToVisibilityConverter ()
		{
			_map = new Dictionary<RouteVehicleVMStates, IList<UIParts>>
			{
				{ RouteVehicleVMStates.Start, new List<UIParts> { UIParts.NoDataText }},
				{ RouteVehicleVMStates.Loading, new List<UIParts> { UIParts.Progress }},
				{ RouteVehicleVMStates.ForecastReceived, new List<UIParts> { UIParts.Station, UIParts.Time }},
				{ RouteVehicleVMStates.ForecastDuplicated, new List<UIParts> { UIParts.Station }},
				{ RouteVehicleVMStates.NoForecast, new List<UIParts> { UIParts.NoDataText }}
			};
		}

		protected override MvxVisibility Convert(RouteVehicleVMStates value, object parameter, CultureInfo culture)
		{
			if (parameter == null)
			{
				return MvxVisibility.Visible;
			}

			UIParts uiPart;
			if (!Enum.TryParse(parameter.ToString(), out uiPart))
			{
				return MvxVisibility.Visible;
			}

			return _map[value].Contains(uiPart) ? MvxVisibility.Visible : MvxVisibility.Collapsed;
		}
	}
}