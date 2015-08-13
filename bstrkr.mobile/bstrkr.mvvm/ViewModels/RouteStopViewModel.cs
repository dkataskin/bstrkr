using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

using bstrkr.core;
using bstrkr.core.collections;
using bstrkr.mvvm.converters;
using bstrkr.mvvm.messages;
using bstrkr.providers;

using Cirrious.MvvmCross.Plugins.Messenger;
using Cirrious.MvvmCross.ViewModels;

namespace bstrkr.mvvm.viewmodels
{
	public class RouteStopViewModel : BusTrackerViewModelBase, ICleanable
	{
		private readonly object _lockObject = new object();
		private readonly ILiveDataProviderFactory _liveDataProviderFactory;
		private readonly RouteInfoToTitleConverter _routeInfoToTitleConverter = new RouteInfoToTitleConverter();
		private readonly ObservableCollection<RouteStopForecastViewModel> _forecast = new ObservableCollection<RouteStopForecastViewModel>();
		private readonly IObservable<long> _intervalObservable;
		private readonly IMvxMessenger _messenger;

		private IDisposable _intervalSubscription;

		private string _routeStopId;
		private string _name;
		private string _description;
		private bool _noData;

		public RouteStopViewModel(ILiveDataProviderFactory liveDataProvider, IMvxMessenger messenger)
		{
			_liveDataProviderFactory = liveDataProvider;
			_intervalObservable = Observable.Interval(TimeSpan.FromMilliseconds(1000));

			this.Forecast = new ReadOnlyObservableCollection<RouteStopForecastViewModel>(_forecast);

			this.ShowOnMapCommand = new MvxCommand(this.ShowOnMap);

			_messenger = messenger;
		}

		public MvxCommand ShowOnMapCommand { get; private set; }

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
		}

		public override void Start()
		{
			base.Start();
			this.Refresh();
		}

		public void CleanUp()
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
							_forecast.Merge(
										forecast.Items,
										vm => vm.VehicleId,
										forecastItem => forecastItem.VehicleId,
										this.CreateFromForecastItem,
										this.UpdateFromForecastItem,
										MergeMode.Full);

							_forecast.Clear();
							foreach(var forecastItem in forecast.Items)
							{
								_forecast.Add(this.CreateFromForecastItem(forecastItem));
							}
						}

						_intervalSubscription = _intervalObservable.Subscribe(this.OnNextInterval);

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

			Task.Delay(TimeSpan.FromSeconds(20))
				.ContinueWith(delayTask => this.Refresh());
		}

		private RouteStopForecastViewModel CreateFromForecastItem(RouteStopForecastItem item)
		{
			return new RouteStopForecastViewModel 
			{
				VehicleId = item.VehicleId,
				VehicleType = item.Route.VehicleTypes.First(),
				ArrivesInSeconds = item.ArrivesInSeconds,
				CurrentlyAt = item.CurrentRouteStopName,
				RouteDisplayName = _routeInfoToTitleConverter.Convert(item.Route.Number, item.Route.VehicleTypes.First()),
				Route = item.Route,
				ParentRoute = item.ParentRoute,
				LastStop = item.LastRouteStopName
			};
		}

		private void UpdateFromForecastItem(RouteStopForecastViewModel vm, RouteStopForecastItem item)
		{
			vm.ArrivesInSeconds = item.ArrivesInSeconds;
			vm.CurrentlyAt = item.CurrentRouteStopName;
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
				}
			});
		}

		private void ShowOnMap()
		{
			_messenger.Publish<ShowRouteStopForecastOnMapMessage>(new ShowRouteStopForecastOnMapMessage(this, this.RouteStopId));
		}
	}
}