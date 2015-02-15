using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using bstrkr.core;
using bstrkr.providers;

using Cirrious.MvvmCross.ViewModels;
using bstrkr.mvvm.converters;

namespace bstrkr.mvvm.viewmodels
{
	public class RouteStopViewModel : BusTrackerViewModelBase
	{
		private readonly ILiveDataProviderFactory _liveDataProviderFactory;
		private readonly RouteNumberToTitleConverter _routeNumberToTileConverter = new RouteNumberToTitleConverter();
		private readonly ObservableCollection<RouteStopForecastViewModel> _forecast = new ObservableCollection<RouteStopForecastViewModel>();

		private string _routeStopId;
		private string _name;
		private string _description;
		private bool _noData;

		public RouteStopViewModel(ILiveDataProviderFactory liveDataProvider)
		{
			_liveDataProviderFactory = liveDataProvider;

			this.Forecast = new ReadOnlyObservableCollection<RouteStopForecastViewModel>(_forecast);
		}

		public string RouteStopId 
		{
			get { return _routeStopId; }
			private set { this.RaiseAndSetIfChanged(ref _routeStopId, value, () => this.RouteStopId); }
		}

		public string Name 
		{
			get { return _name; }
			private set { this.RaiseAndSetIfChanged(ref _name, value, () => this.Name); }
		}

		public string Description 
		{
			get { return _description; }
			private set { this.RaiseAndSetIfChanged(ref _description, value, () => this.Description); }
		}

		public bool NoData
		{
			get { return _noData; }
			private set { this.RaiseAndSetIfChanged(ref _noData, value, () => this.NoData); }
		}

		public ReadOnlyObservableCollection<RouteStopForecastViewModel> Forecast { get; private set; }

		public void Init(string id, string name, string description)
		{
			this.RouteStopId = id;
			this.Name = name;
			this.Description = description;

			this.Refresh();
		}

		private void Refresh()
		{
			var provider = _liveDataProviderFactory.GetCurrentProvider();
			if (provider != null)
			{
				this.Dispatcher.RequestMainThreadAction(() => this.IsBusy = true);
				provider.GetRouteStopForecastAsync(this.RouteStopId)
						.ContinueWith(this.ShowForecast)
						.ConfigureAwait(false);
			}
		}

		private void ShowForecast(Task<RouteStopForecast> task)
		{
			if (task.Status == TaskStatus.RanToCompletion)
			{
				var forecast = task.Result;
				if (forecast != null && forecast.Items.Any())
				{
					this.Dispatcher.RequestMainThreadAction(() =>
					{
						_forecast.Clear();

						foreach(var forecastItem in forecast.Items)
						{
							_forecast.Add(this.CreateFromForecastItem(forecastItem));
						}

						this.NoData = false;
						this.IsBusy = false;
					});
				}
				else
				{
					this.Dispatcher.RequestMainThreadAction(() =>
					{
						_forecast.Clear();
						this.IsBusy = false;
						this.NoData = true;
					});
				}
			}
		}

		private RouteStopForecastViewModel CreateFromForecastItem(RouteStopForecastItem item)
		{
			return new RouteStopForecastViewModel 
			{
				VehicleId = item.VehicleId,
				ArrivesInSeconds = item.ArrivesInSeconds,
				CurrentlyAt = item.CurrentRouteStopName,
				RouteDisplayName = _routeNumberToTileConverter.Convert(item.Route.Number, item.Route.VehicleTypes.First()),
				Route = item.Route,
				ParentRoute = item.ParentRoute,
				LastStop = item.LastRouteStopName
			};
		}
	}
}