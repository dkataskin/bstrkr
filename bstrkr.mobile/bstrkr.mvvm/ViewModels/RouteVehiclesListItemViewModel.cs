using System;
using System.Linq;
using System.Threading.Tasks;

using bstrkr.core;
using bstrkr.mvvm.viewmodels;
using bstrkr.providers;

using Xamarin;

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
			private set
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
			private set
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
			private set
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
			private set
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
			if (provider == null)
			{
				return;
			}

			this.Dispatcher.RequestMainThreadAction(() => this.IsBusy = true);

			try
			{
				var forecast = await provider.GetVehicleForecastAsync(this.Vehicle)
											 .ConfigureAwait(false);

				if (forecast.Items.Any())
				{
					this.Dispatcher.RequestMainThreadAction(() => 
					{
						this.RouteStopId = forecast.Items.First().RouteStop.Id;
						this.RouteStopName = forecast.Items.First().RouteStop.Name;
						this.RouteStopDescription = forecast.Items.First().RouteStop.Description;
					});
				}
			} 
			catch (Exception e)
			{
				Insights.Report(e, ReportSeverity.Warning);
			}
			finally
			{
				this.Dispatcher.RequestMainThreadAction(() => this.IsBusy = false);
			}
		}
	}
}