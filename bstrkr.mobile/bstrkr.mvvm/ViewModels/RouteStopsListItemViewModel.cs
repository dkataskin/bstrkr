using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using bstrkr.core;

namespace bstrkr.mvvm.viewmodels
{
	public class RouteStopsListItemViewModel : BusTrackerViewModelBase
	{
		private string _name;

		public RouteStopsListItemViewModel(string name, IEnumerable<RouteStop> stops)
		{
			this.Name = name;
			this.Stops = new ReadOnlyCollection<RouteStop>(stops.ToList());
		}

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

		public IReadOnlyCollection<RouteStop> Stops { get; private set; }
	}
}