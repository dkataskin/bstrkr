using System;
using System.Collections.ObjectModel;
using System.Linq;

using bstrkr.mvvm.navigation;
using bstrkr.mvvm.viewmodels;

using Cirrious.CrossCore.Platform;
using Cirrious.MvvmCross.ViewModels;

using Newtonsoft.Json;

namespace bstrkr.mvvm.viewmodels
{
	public class SetRouteViewModel : BusTrackerViewModelBase
	{
		private readonly ObservableCollection<string> _routesObservable = new ObservableCollection<string>();

		private RouteListNavParam _routesListNavParam;

		public SetRouteViewModel()
		{
			this.Routes = new ReadOnlyObservableCollection<string>(_routesObservable);

			this.SelectRouteCommand = new MvxCommand<int>(this.SelectRoute);
		}

		public MvxCommand<int> SelectRouteCommand { get; private set; }

		public ReadOnlyObservableCollection<string> Routes { get; private set; }

		public void Init(string routes)
		{
			try
			{
				_routesListNavParam = JsonConvert.DeserializeObject<RouteListNavParam>(routes);
				if (_routesListNavParam != null && 
					_routesListNavParam.Routes != null &&
					_routesListNavParam.Routes.Any())
				{
					foreach (var route in _routesListNavParam.Routes) 
					{
						_routesObservable.Add(route.Stops);
					}
				}
			}
			catch(Exception e)
			{
				MvxTrace.Trace("An error occured while deserializing route list: {0}", e);
			}
		}

		private void SelectRoute(int index)
		{
			var selectedRoute = _routesListNavParam.Routes[index];
			this.ShowViewModel<RouteVehiclesViewModel>(new 
			{ 
				routeId = selectedRoute.Id,
				routeName = selectedRoute.Name,
				routeNumber = selectedRoute.Number,
				vehicleType = selectedRoute.VehicleType
			});
		}
	}
}