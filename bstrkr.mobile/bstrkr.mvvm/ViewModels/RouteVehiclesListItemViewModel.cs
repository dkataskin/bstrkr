using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

using bstrkr.core;
using bstrkr.mvvm.viewmodels;
using bstrkr.providers;

using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;

using Stateless;

using Xamarin;

namespace bstrkr.mvvm.viewmodels
{
	public class RouteVehiclesListItemViewModel : BusTrackerViewModelBase
	{
		private readonly ILiveDataProviderFactory _liveDataProviderFactory;
		private readonly object _lockObject = new object();

		private readonly ObservableCollection<VehicleForecastListItemViewModel> _forecast = 
			new ObservableCollection<VehicleForecastListItemViewModel>();
		private readonly StateMachine<RouteVehicleVMStates, RouteVehicleVMTriggers> _stateMachine;

		private VehicleForecastListItemViewModel _nextStopForecast;
		private string _prevRouteStopId;

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

			this.Forecast = new ReadOnlyObservableCollection<VehicleForecastListItemViewModel>(_forecast);
		}

		public MvxCommand UpdateForecastCommand { get; private set; }

		public MvxCommand CountdownCommand { get; private set; }

		public Vehicle Vehicle { get; set; }

		public RouteVehicleVMStates State
		{
			get { return _stateMachine.State; }
		}

		public VehicleForecastListItemViewModel NextStopForecast 
		{ 
			get { return _nextStopForecast; } 
			private set
			{
				if (_nextStopForecast != value)
				{
					_nextStopForecast = value;
					this.RaisePropertyChanged(() => this.NextStopForecast);
				}
			}
		}

		public ReadOnlyObservableCollection<VehicleForecastListItemViewModel> Forecast { get; private set; }

		public void Init(string id, string carPlate, VehicleTypes vehicleType, string routeId, string routeDisplayName)
		{
			this.Vehicle = new Vehicle 
			{
				Id = id,
				CarPlate = carPlate,
				Type = vehicleType,
				RouteInfo = new VehicleRouteInfo 
				{
					RouteId = routeId,
					DisplayName = routeDisplayName
				}
			};
		}

		public override void Start()
		{
			base.Start();
			this.CountdownCommand.Execute();
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
					if (forecast.Items == null || !forecast.Items.Any())
					{
						this.NextStopForecast = null;
						_stateMachine.Fire(RouteVehicleVMTriggers.NoForecastDataReturned);
						return;
					}

					lock(_lockObject)
					{
						this.NextStopForecast = null;
						_prevRouteStopId = string.Empty;
						_forecast.Clear();

						foreach (var forecastItem in forecast.Items) 
						{
							var vm = this.CreateFromForecastItem(forecastItem);
							if (vm != null)
							{
								_forecast.Add(vm);
							}
						}

						this.NextStopForecast = _forecast.FirstOrDefault();
						if (this.NextStopForecast != null)
						{
							_prevRouteStopId = this.NextStopForecast.RouteStopId;
						}

						if (this.NextStopForecast != null && (this.NextStopForecast.ArrivesInSeconds == 0 ||
							(this.NextStopForecast.ArrivesInSeconds < 10 && string.Equals(_prevRouteStopId, this.NextStopForecast.RouteStopId))))
						{
							_stateMachine.Fire(RouteVehicleVMTriggers.DuplicateForecastReturned);
						}
						else
						{
							_stateMachine.Fire(RouteVehicleVMTriggers.ForecastReturned);
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
					foreach (var forecastVM in this.Forecast) 
					{
						forecastVM.CountdownCommand.Execute();
					}

					var toRemove = this.Forecast.Where(x => x.ArrivesInSeconds == 0);
					foreach (var forecastVMToRemove in toRemove) 
					{
						_forecast.Remove(forecastVMToRemove);
					}

					if (this.NextStopForecast != null)
					{
						if (this.NextStopForecast.ArrivesInSeconds == 0)
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

		private VehicleForecastListItemViewModel CreateFromForecastItem(VehicleForecastItem forecastItem)
		{
			if (forecastItem == null || forecastItem.RouteStop == null)
			{
				return null;
			}

			var vm = Mvx.IocConstruct<VehicleForecastListItemViewModel>();
			vm.UpdateFromForecastItem(forecastItem);

			return vm;
		}
	}
}