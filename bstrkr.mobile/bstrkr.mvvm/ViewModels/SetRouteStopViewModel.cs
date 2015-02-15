using System;
using System.Collections.ObjectModel;
using System.Linq;

using bstrkr.mvvm.navigation;
using bstrkr.providers;

using Cirrious.CrossCore.Platform;
using Cirrious.MvvmCross.ViewModels;

using Newtonsoft.Json;

namespace bstrkr.mvvm.viewmodels
{
	public class SetRouteStopViewModel : BusTrackerViewModelBase
	{
		private readonly ObservableCollection<string> _routeStopsObservable = new ObservableCollection<string>();

		private RouteStopListNavParam _routeStopListNavParam;

		public SetRouteStopViewModel()
		{
			this.RouteStops = new ReadOnlyObservableCollection<string>(_routeStopsObservable);

			this.SelectRouteStopCommand = new MvxCommand<int>(this.SelectRouteStop);
		}

		public MvxCommand<int> SelectRouteStopCommand { get; private set; }

		public ReadOnlyObservableCollection<string> RouteStops { get; private set; }

		public void Init(string stops)
		{
			try
			{
				_routeStopListNavParam = JsonConvert.DeserializeObject<RouteStopListNavParam>(stops);
				if (_routeStopListNavParam != null && 
					_routeStopListNavParam.RouteStops != null &&
					_routeStopListNavParam.RouteStops.Any())
				{
					foreach (var routeStop in _routeStopListNavParam.RouteStops) 
					{
						_routeStopsObservable.Add(routeStop.RouteStopDescription);
					}
				}
			}
			catch(Exception e)
			{
				MvxTrace.Trace("An error occured while deserializing route stop list: {0}", e);
			}
		}

		private void SelectRouteStop(int index)
		{
			var selectedRouteStop = _routeStopListNavParam.RouteStops[index];
			this.ShowViewModel<RouteStopViewModel>(new 
			{ 
				id = selectedRouteStop.RouteStopId,
				name = selectedRouteStop.RouteStopName,
				description = selectedRouteStop.RouteStopDescription
			});
		}
	}
}