using System;

using bstrkr.mvvm.viewmodels;
using bstrkr.core;

namespace bstrkr.mvvm.viewmodels
{
	public class RouteForecastViewModel : BusTrackerViewModelBase
	{
		public RouteStop RouteStop { get; set; }

		public int ArrivesInSeconds { get; set; }
	}
}