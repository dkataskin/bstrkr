using System;
using System.Threading.Tasks;

using bstrkr.core;
using bstrkr.mvvm.viewmodels;
using bstrkr.providers;

namespace bstrkr.mvvm.viewmodels
{
	public class RouteVehiclesListItemViewModel : BusTrackerViewModelBase
	{
		private readonly ILiveDataProviderFactory _liveDataProviderFactory;

		private int _arrivesInSeconds;
		private string _routeStopId;
		private string _routeStopName;
		private string _routeStopDescription;

		public RouteVehiclesListItemViewModel(ILiveDataProviderFactory liveDataProviderFactory)
		{
			_liveDataProviderFactory = liveDataProviderFactory;
		}

		public Vehicle Vehicle { get; set; }

		public int ArrivesInSeconds 
		{ 
			get { return _arrivesInSeconds; } 
			set
			{
				if (_arrivesInSeconds != value)
				{
					_arrivesInSeconds = value;
					this.RaisePropertyChanged(() => this.ArrivesInSeconds);
				}
			}
		}

		public string RouteStopId 
		{ 
			get { return _routeStopId; } 
			set
			{
				if (_routeStopId != value)
				{
					_routeStopId = value;
					this.RaisePropertyChanged(() => this.RouteStopId);
				}
			}
		}

		public string RouteStopName 
		{ 
			get { return _routeStopName; }
			set
			{
				if (_routeStopName != value)
				{
					_routeStopName = value;
					this.RaisePropertyChanged(() => this.RouteStopName);
				}
			}
		}

		public string RouteStopDescription 
		{ 
			get { return _routeStopDescription; } 
			set
			{
				if (_routeStopDescription != value)
				{
					_routeStopDescription = value;
					this.RaisePropertyChanged(() => this.RouteStopDescription);
				}
			}
		}

		public async Task UpdateAsync()
		{
			var provider = _liveDataProviderFactory.GetCurrentProvider();
			if (provider != null)
			{
				return;
			}

			this.Dispatcher.RequestMainThreadAction(() => this.IsBusy = true);

			var forecast = await provider.GetVehicleForecastAsync(this.Vehicle);
			this.Dispatcher.RequestMainThreadAction(() => this.IsBusy = false);
		}
	}
}