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
	public class RouteVehiclesListItemViewModel : BusTrackerViewModelBase
	{
		private readonly ILiveDataProviderFactory _liveDataProviderFactory;
		private readonly object _lockObject = new object();

		private readonly StateMachine<RouteVehicleVMStates, RouteVehicleVMTriggers> _stateMachine;

		private int _arrivesInSeconds;
		private string _routeStopId;
		private string _prevRouteStopId;
		private string _routeStopName;
		private string _routeStopDescription;

		public RouteVehiclesListItemViewModel(ILiveDataProviderFactory liveDataProviderFactory)
		{
			_liveDataProviderFactory = liveDataProviderFactory;

			this.UpdateForecastCommand = new MvxCommand(
												() => _stateMachine.Fire(RouteVehicleVMTriggers.ForecastRequested), 
												() => _stateMachine.CanFire(RouteVehicleVMTriggers.ForecastRequested));

			this.CountdownCommand = new MvxCommand(
												this.Countdown,
												() => _stateMachine.IsInState(RouteVehicleVMStates.ForecastReceived));

			_stateMachine = new StateMachine<RouteVehicleVMStates, RouteVehicleVMTriggers>(RouteVehicleVMStates.Start);
			_stateMachine.OnTransitioned(sm => this.Dispatcher.RequestMainThreadAction(() => this.RaisePropertyChanged(() => this.State)));

			_stateMachine.Configure(RouteVehicleVMStates.Start)
			  			 .Permit(RouteVehicleVMTriggers.ForecastRequested, RouteVehicleVMStates.Loading);

			_stateMachine.Configure(RouteVehicleVMStates.Loading)
						 .OnEntry(this.RequestForecast)
						 .Permit(RouteVehicleVMTriggers.ForecastReturned, RouteVehicleVMStates.ForecastReceived)
						 .Permit(RouteVehicleVMTriggers.NoForecastDataReturned, RouteVehicleVMStates.NoForecast)
						 .Permit(RouteVehicleVMTriggers.DuplicateForecastReturned, RouteVehicleVMStates.ForecastDuplicated)
						 .Permit(RouteVehicleVMTriggers.RequestFailed, RouteVehicleVMStates.NoForecast);

			_stateMachine.Configure(RouteVehicleVMStates.ForecastReceived)
						 .Permit(RouteVehicleVMTriggers.ForecastRequested, RouteVehicleVMStates.Loading);

			_stateMachine.Configure(RouteVehicleVMStates.ForecastDuplicated)
						 .OnEntry(this.PauseAndRequest)
						 .Permit(RouteVehicleVMTriggers.ForecastRequested, RouteVehicleVMStates.Loading);

			_stateMachine.Configure(RouteVehicleVMStates.NoForecast)
						 .OnEntry(this.PauseAndRequest)
						 .Permit(RouteVehicleVMTriggers.ForecastRequested, RouteVehicleVMStates.Loading);
		}

		public MvxCommand UpdateForecastCommand { get; private set; }

		public MvxCommand CountdownCommand { get; private set; }

		public Vehicle Vehicle { get; set; }

		public RouteVehicleVMStates State
		{
			get { return _stateMachine.State; }
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

		protected override void OnIsBusyChanged()
		{
			base.OnIsBusyChanged();
			this.UpdateForecastCommand.RaiseCanExecuteChanged();
		}

		private void RequestForecast()
		{
			this.RequestForecastAsync()
				.ConfigureAwait(false)
				.GetAwaiter()
				.GetResult();
		}

		private async Task RequestForecastAsync()
		{
			var provider = _liveDataProviderFactory.GetCurrentProvider();
			if (provider == null)
			{
				return;
			}

			this.Dispatcher.RequestMainThreadAction(() => 
			{
				this.IsBusy = true;
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
							_prevRouteStopId = this.RouteStopId;
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

								_stateMachine.Fire(RouteVehicleVMTriggers.NoForecastDataReturned);
								return;
							}

							if (this.ArrivesInSeconds == 0 || 
								(this.ArrivesInSeconds < 10 && string.Equals(_prevRouteStopId, this.RouteStopId)))
							{
								_stateMachine.Fire(RouteVehicleVMTriggers.DuplicateForecastReturned);
							}
							else
							{
								_stateMachine.Fire(RouteVehicleVMTriggers.ForecastReturned);
							}
						}
						else
						{
							_stateMachine.Fire(RouteVehicleVMTriggers.NoForecastDataReturned);
						}
					}
				});
			} 
			catch (Exception e)
			{
				Insights.Report(e, ReportSeverity.Warning);
				_stateMachine.Fire(RouteVehicleVMTriggers.RequestFailed);
			}
			finally
			{
				this.Dispatcher.RequestMainThreadAction(() => this.IsBusy = false);
			}
		}

		private void Countdown()
		{
			this.Dispatcher.RequestMainThreadAction(() =>
			{
				lock (_lockObject)
				{
					if (this.ArrivesInSeconds > 0)
					{
						this.ArrivesInSeconds--;

						if (this.ArrivesInSeconds == 0)
						{
							Task.Factory.StartNew(() => _stateMachine.Fire(RouteVehicleVMTriggers.ForecastRequested));
						}
					}
				}
			});
		}

		private void PauseAndRequest()
		{
			Task.Delay(TimeSpan.FromSeconds(30))
				.ContinueWith(task => _stateMachine.Fire(RouteVehicleVMTriggers.ForecastRequested));
		}
	}
}