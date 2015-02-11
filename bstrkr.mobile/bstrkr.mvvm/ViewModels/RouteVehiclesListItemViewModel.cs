using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

using bstrkr.core;
using bstrkr.mvvm.viewmodels;
using bstrkr.providers;

using Cirrious.MvvmCross.ViewModels;

using Stateless;

using Xamarin;

namespace bstrkr.mvvm.viewmodels
{
	public enum RouteVehicleVMStates
	{
		Start,
		Loading,
		LoadComplete
	}

	public enum RouteVehicleVMTriggers
	{
		ForecastRequested,
		ForecastReturned,
		NoForecastDataReturned,
		DuplicateForecastReturned,
		RequestFailed
	}

	public class RouteVehiclesListItemViewModel : BusTrackerViewModelBase, IDisposable
	{
		private readonly ILiveDataProviderFactory _liveDataProviderFactory;
		private readonly object _lockObject = new object();
		private readonly IObservable<long> _intervalObservable;
		private readonly IDisposable _intervalSubscription;

		private int _arrivesInSeconds;
		private string _routeStopId;
		private string _routeStopName;
		private string _routeStopDescription;
		private bool _noData = true;

		public RouteVehiclesListItemViewModel(ILiveDataProviderFactory liveDataProviderFactory)
		{
			_liveDataProviderFactory = liveDataProviderFactory;
			_intervalObservable = Observable.Interval(TimeSpan.FromMilliseconds(1000));
			_intervalSubscription = _intervalObservable.Subscribe(this.OnNextInterval);

			this.UpdateForecastCommand = new MvxCommand(this.Update, () => !this.IsBusy);

			var sm = new StateMachine<RouteVehicleVMStates, RouteVehicleVMTriggers>(RouteVehicleVMStates.Start);
			sm.Configure(RouteVehicleVMStates.Start)
			  .Permit(RouteVehicleVMTriggers.ForecastRequested, RouteVehicleVMStates.Loading);

			sm.Configure(RouteVehicleVMStates.Loading)
			  .Permit(RouteVehicleVMTriggers.ForecastReturned, RouteVehicleVMStates.LoadComplete)
			  .Permit(RouteVehicleVMTriggers.NoForecastDataReturned, RouteVehicleVMStates.LoadComplete)
			  .Permit(RouteVehicleVMTriggers.DuplicateForecastReturned, RouteVehicleVMStates.LoadComplete)
			  .Permit(RouteVehicleVMTriggers.RequestFailed, RouteVehicleVMStates.LoadComplete);

			sm.Configure(RouteVehicleVMStates.LoadComplete)
			  .OnEntryFrom(RouteVehicleVMTriggers.ForecastReturned, this.UpdateForecast)
			  .OnEntryFrom(RouteVehicleVMTriggers.NoForecastDataReturned, this.ClearForecast)
			  .Permit(RouteVehicleVMTriggers.ForecastRequested, RouteVehicleVMStates.Loading);
		}

		public MvxCommand UpdateForecastCommand { get; private set; }

		public Vehicle Vehicle { get; set; }

		public bool NoData 
		{
			get { return _noData; }
			private set
			{
				if (_noData != value)
				{
					_noData = value;
					this.RaisePropertyChanged(() => this.NoData);
					this.RaisePropertyChanged(() => this.HasForecast);
				}
			}
		}

		public bool HasForecast
		{
			get { return !this.IsBusy && !this.NoData; }
		}

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

		public void Dispose()
		{
			if (_intervalSubscription != null)
			{
				_intervalSubscription.Dispose();
			}
		}

		protected override void OnIsBusyChanged()
		{
			base.OnIsBusyChanged();
			this.RaisePropertyChanged(() => this.HasForecast);
			UpdateForecastCommand.RaiseCanExecuteChanged();
		}

		private void Update()
		{
			this.UpdateAsync()
				.ConfigureAwait(false)
				.GetAwaiter()
				.GetResult();
		}

		private async Task UpdateAsync()
		{
			var provider = _liveDataProviderFactory.GetCurrentProvider();
			if (provider == null)
			{
				return;
			}

			this.Dispatcher.RequestMainThreadAction(() => 
			{
				this.IsBusy = true;
				this.NoData = false;
			});

			try
			{
				var forecast = await provider.GetVehicleForecastAsync(this.Vehicle)
											 .ConfigureAwait(false);

				this.Dispatcher.RequestMainThreadAction(() => 
				{
					var forecastItem = forecast.Items.FirstOrDefault();
					lock(_lockObject)
					{
						if (forecastItem != null)
						{
							this.ArrivesInSeconds = forecastItem.ArrivesInSec;
							if (forecastItem.RouteStop != null)
							{
								this.RouteStopId = forecastItem.RouteStop.Id;
								this.RouteStopName = forecastItem.RouteStop.Name;
								this.RouteStopDescription = forecastItem.RouteStop.Description;
							}
							else
							{
								this.RouteStopId = string.Empty;
								this.RouteStopName = string.Empty;
								this.RouteStopDescription = string.Empty;
							}
						}
						else
						{
							this.NoData = true;
						}
					}
				});
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

		private void OnNextInterval(long interval)
		{
			this.Dispatcher.RequestMainThreadAction(() =>
			{
				lock (_lockObject)
				{
					if (this.ArrivesInSeconds > 0)
					{
						this.ArrivesInSeconds--;
					}

					if (this.ArrivesInSeconds == 0 && this.HasForecast)
					{
						this.UpdateAsync();
					}
				}
			});
		}

		private void UpdateForecast()
		{
		}

		private void ClearForecast()
		{
		}
	}
}