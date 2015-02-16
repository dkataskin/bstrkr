using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using bstrkr.core;
using bstrkr.providers;

using Cirrious.MvvmCross.ViewModels;
using bstrkr.mvvm.converters;
using System.Reactive.Linq;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Reactive;

namespace bstrkr.mvvm.viewmodels
{
	public class RouteStopViewModel : BusTrackerViewModelBase, IDisposable
	{
		private class MyObservable : IObservable<int>
		{
			public IDisposable Subscribe(IObserver<int> observer)
			{
				throw new NotImplementedException();
			}
		}

		private const int _minTimeBetweenRequestsInSeconds = 30;
		private readonly object _lockObject = new object();
		private readonly ILiveDataProviderFactory _liveDataProviderFactory;
		private readonly RouteNumberToTitleConverter _routeNumberToTileConverter = new RouteNumberToTitleConverter();
		private readonly ObservableCollection<RouteStopForecastViewModel> _forecast = new ObservableCollection<RouteStopForecastViewModel>();
		private readonly IObservable<long> _intervalObservable;

		private DateTime _lastTimeRequested;
		private IDisposable _intervalSubscription;

		private string _routeStopId;
		private string _name;
		private string _description;
		private bool _noData;

		public RouteStopViewModel(ILiveDataProviderFactory liveDataProvider)
		{
			_liveDataProviderFactory = liveDataProvider;
			_intervalObservable = Observable.Interval(TimeSpan.FromMilliseconds(1000));

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

		public void Dispose()
		{
			if (_intervalSubscription != null)
			{
				_intervalSubscription.Dispose();
			}
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
						if (_intervalSubscription != null)
						{
							_intervalSubscription.Dispose();
						}

						lock(_lockObject)
						{
							_forecast.Clear();
							foreach(var forecastItem in forecast.Items)
							{
								_forecast.Add(this.CreateFromForecastItem(forecastItem));
							}
						}

						_intervalSubscription = _intervalObservable.Subscribe(this.OnNextInterval);

						this.NoData = false;
						this.IsBusy = false;

						_lastTimeRequested = DateTime.UtcNow;
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

		private void OnNextInterval(long interval)
		{
			this.Dispatcher.RequestMainThreadAction(() =>
			{
				lock (_lockObject)
				{
					var vmsToRemove = new List<RouteStopForecastViewModel>();
					foreach (var vm in this.Forecast) 
					{
						vm.CountdownCommand.Execute();

						if (vm.ArrivesInSeconds == 0)
						{
							vmsToRemove.Add(vm);
						}
					}

					foreach (var vmToRemove in vmsToRemove) 
					{
						_forecast.Remove(vmToRemove);
					}

					var now = DateTime.UtcNow;
					if (vmsToRemove.Any() && 
						!this.IsBusy && 
						(now - _lastTimeRequested).TotalSeconds > _minTimeBetweenRequestsInSeconds)
					{
						Task.Factory.StartNew(this.Refresh)
								 	.ConfigureAwait(false);
					}
				}
			});
		}
	}
}