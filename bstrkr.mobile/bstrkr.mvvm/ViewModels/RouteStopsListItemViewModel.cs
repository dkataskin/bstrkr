using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using bstrkr.core;
using bstrkr.core.spatial;

using Cirrious.MvvmCross.ViewModels;

namespace bstrkr.mvvm.viewmodels
{
	public class RouteStopsListItemViewModel : BusTrackerViewModelBase
	{
		private string _name;
		private int _distanceInMeters;

		public RouteStopsListItemViewModel(string name, IEnumerable<RouteStop> stops)
		{
			this.Name = name;
			this.Stops = new ReadOnlyCollection<RouteStop>(stops.ToList());

			this.CalculateDistanceCommand = new MvxCommand<GeoPoint>(this.CalculateDistance);
		}

		public MvxCommand<GeoPoint> CalculateDistanceCommand { get; private set; }

		public string Name 
		{ 
			get
			{
				return _name;
			} 

			private set
			{
				if (!string.Equals(_name, value))
				{
					_name = value;
					this.RaisePropertyChanged(() => this.Name);
				}
			} 
		}

		public int DistanceInMeters
		{
			get { return _distanceInMeters;}
			private set { this.RaiseAndSetIfChanged(ref _distanceInMeters, value, () => this.DistanceInMeters); }
		}

		public IReadOnlyCollection<RouteStop> Stops { get; private set; }

		private void CalculateDistance(GeoPoint location)
		{
			this.DistanceInMeters = Convert.ToInt32(Math.Round(this.Stops.Min(x => location.DistanceTo(x.Location) * 1000)));
		}
	}
}