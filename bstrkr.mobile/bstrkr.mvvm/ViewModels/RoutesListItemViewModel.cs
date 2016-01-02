using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Cirrious.MvvmCross.ViewModels;

using bstrkr.core;
using System.Text;

namespace bstrkr.mvvm.viewmodels
{
	public class RoutesListItemViewModel : BusTrackerViewModelBase
	{
		private const string TwoRouteStopsFormat = "{0} — {1}";
		private const string ThreeRouteStopsFormat = "{0} — {1} — {2}";

		private string _name;
		private string _number;
		private string _routeStops;
		private VehicleTypes _vehicleType;
		private ReadOnlyObservableCollection<Route> _routes;

		public RoutesListItemViewModel(IEnumerable<Route> routes, string name, string number, VehicleTypes vehicleType)
		{
			this.Routes = new ReadOnlyObservableCollection<Route>(new ObservableCollection<Route>(routes));
			this.Name = name;
			this.Number = number;
			this.VehicleType = vehicleType;
			this.RouteStops = this.GetRouteStops(this.Routes);
		}

		public string Name 
		{ 
			get { return _name; } 
			private set { this.RaiseAndSetIfChanged(ref _name, value, () => this.Name); } 
		}

		public string Number 
		{ 
			get { return _number; } 
			private set { this.RaiseAndSetIfChanged(ref _number, value, () => this.Number); } 
		}

		public string RouteStops 
		{ 
			get { return _routeStops; } 
			private set { this.RaiseAndSetIfChanged(ref _routeStops, value, () => this.RouteStops); } 
		}

		public VehicleTypes VehicleType
		{
			get { return _vehicleType; }
			private set { this.RaiseAndSetIfChanged(ref _vehicleType, value, () => this.VehicleType); }
		}

		public ReadOnlyObservableCollection<Route> Routes 
		{ 
			get { return _routes; }
			private set { this.RaiseAndSetIfChanged(ref _routes, value, () => this.Routes); } 
		}

		private string GetRouteStops(IEnumerable<Route> routes)
		{
			if (routes.Count() == 1)
			{
				return string.Format(TwoRouteStopsFormat, routes.First().FirstStop, routes.Last().LastStop);
			}

			var stops = routes.SelectMany(r => new[] { r.FirstStop, r.LastStop });
			var routeStopsIds = stops.Select(s => s.Id).Distinct().ToList();
			if (routeStopsIds.Count() == 1)
			{
				return routes.First().FirstStop.Name;
			}

			if (routeStopsIds.Count() == 2)
			{
				var firstStop = stops.First(s => s.Id.Equals(routeStopsIds[0]));
				var lastStop = stops.First(s => s.Id.Equals(routeStopsIds[1]));

				return string.Format(ThreeRouteStopsFormat, firstStop.Name, lastStop.Name, firstStop.Name);
			}

			return string.Join(", ", routeStopsIds.Select(id => stops.First(s => s.Id.Equals(id)).Name).ToList());
		}
	}
}