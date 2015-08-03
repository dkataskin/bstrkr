using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

using bstrkr.core;
using bstrkr.mvvm.messages;
using bstrkr.mvvm.viewmodels;
using bstrkr.providers;

using Cirrious.CrossCore;
using Cirrious.MvvmCross.Plugins.Messenger;
using Cirrious.MvvmCross.ViewModels;

using Stateless;

using Xamarin;

namespace bstrkr.mvvm.viewmodels
{
	public class VehicleForecastViewModel : BusTrackerViewModelBase, ICleanable
	{
		private const int StopLengthInSeconds = 10;

		private readonly IMvxMessenger _messenger;
		private readonly ILiveDataProviderFactory _liveDataProviderFactory;
		private readonly object _lockObject = new object();

		private readonly ObservableCollection<VehicleForecastListItemViewModel> _forecast = 
			new ObservableCollection<VehicleForecastListItemViewModel>();
		private readonly StateMachine<RouteVehicleVMStates, RouteVehicleVMTriggers> _stateMachine;

		private bool _runUpdates;
		private Task _runUpdatesTask;
		private CancellationTokenSource _tokenSource;
		private CancellationToken _cancellationToken;

		private VehicleForecastListItemViewModel _nextStopForecast;
		private Vehicle _vehicle;
		private Route _route;

		public VehicleForecastViewModel(ILiveDataProviderFactory liveDataProviderFactory, IMvxMessenger messenger)
		{
			_messenger = messenger;
			_liveDataProviderFactory = liveDataProviderFactory;

			this.UpdateForecastCommand = new MvxCommand(
												() => _stateMachine.Fire(RouteVehicleVMTriggers.ForecastRequested), 
												() => _stateMachine.CanFire(RouteVehicleVMTriggers.ForecastRequested));

			this.CountdownCommand = new MvxCommand(
												this.Countdown,
												() => _stateMachine.IsInState(RouteVehicleVMStates.ForecastReceived));

			this.ShowOnMapCommand = new MvxCommand(this.ShowOnMap);

			_stateMachine = new StateMachine<RouteVehicleVMStates, RouteVehicleVMTriggers>(RouteVehicleVMStates.Start);
			_stateMachine.OnTransitioned(sm => this.Dispatcher.RequestMainThreadAction(() => this.RaisePropertyChanged(() => this.State)));

			_stateMachine.Configure(RouteVehicleVMStates.Start)
			  			 .Permit(RouteVehicleVMTriggers.ForecastRequested, RouteVehicleVMStates.Loading);

			_stateMachine.Configure(RouteVehicleVMStates.Loading)
						 .OnEntry(this.RequestForecast)
						 .Permit(RouteVehicleVMTriggers.ForecastReturned, RouteVehicleVMStates.ForecastReceived)
						 .Permit(RouteVehicleVMTriggers.NoForecastDataReturned, RouteVehicleVMStates.NoForecast)
						 .Permit(RouteVehicleVMTriggers.RequestFailed, RouteVehicleVMStates.NoForecast);

			_stateMachine.Configure(RouteVehicleVMStates.ForecastReceived)
						 .Permit(RouteVehicleVMTriggers.ForecastRequested, RouteVehicleVMStates.Loading);

			_stateMachine.Configure(RouteVehicleVMStates.NoForecast)
						 .OnEntry(this.PauseAndRequest)
						 .Permit(RouteVehicleVMTriggers.ForecastRequested, RouteVehicleVMStates.Loading);

			this.Forecast = new ReadOnlyObservableCollection<VehicleForecastListItemViewModel>(_forecast);
		}

		public MvxCommand UpdateForecastCommand { get; private set; }

		public MvxCommand CountdownCommand { get; private set; }

		public MvxCommand ShowOnMapCommand { get; private set; }

		public Vehicle Vehicle 
		{ 
			get { return _vehicle; } 
			private set { this.RaiseAndSetIfChanged(ref _vehicle, value, () => Vehicle); }
		}

		public Route Route
		{
			get { return _route; }
			private set { this.RaiseAndSetIfChanged(ref _route, value, () => Route); }
		}

		public RouteVehicleVMStates State
		{
			get { return _stateMachine.State; }
		}

		public ReadOnlyObservableCollection<VehicleForecastListItemViewModel> Forecast { get; private set; }

		public VehicleForecastListItemViewModel NextStopForecast
		{
			get { return _nextStopForecast; }
			private set { this.RaiseAndSetIfChanged(ref _nextStopForecast, value, () => this.NextStopForecast); }
		}

		public void Init(
					string id, 
					string carPlate, 
					VehicleTypes vehicleType, 
					string routeId, 
					string routeNumber,
					string routeDisplayName,
					bool runUpdates)
		{
			_runUpdates = runUpdates;

			this.Vehicle = new Vehicle 
			{
				Id = id,
				CarPlate = carPlate,
				Type = vehicleType,
				RouteInfo = new VehicleRouteInfo 
				{
					RouteId = routeId,
					RouteNumber = routeNumber,
					DisplayName = routeDisplayName
				}
			};
		}

		public void InitWithData(Vehicle vehicle, Route route, bool runUpdates)
		{
			_runUpdates = runUpdates;
			this.Vehicle = vehicle;
			this.Route = route;
		}

		public override void Start()
		{
			base.Start();

			var provider = _liveDataProviderFactory.GetCurrentProvider();
			if (provider != null && this.Vehicle.RouteInfo != null && 
				!string.IsNullOrEmpty(this.Vehicle.RouteInfo.RouteId))
			{
				provider.GetRouteAsync(this.Vehicle.RouteInfo.RouteId)
						.ContinueWith(task =>
						{
							if (task.Status == TaskStatus.RanToCompletion)
							{
								this.Dispatcher.RequestMainThreadAction(() => this.Route = task.Result);
								this.UpdateForecastCommand.Execute();
							}
						})
						.ConfigureAwait(false);
			}

			if (_runUpdates)
			{
				_tokenSource = new CancellationTokenSource();
				_cancellationToken = _tokenSource.Token;
				_runUpdatesTask = Task.Factory.StartNew(() => this.RunUpdates(_cancellationToken), _cancellationToken);
			}
		}

		public void CleanUp()
		{
			if (_tokenSource != null)
			{
				_tokenSource.Cancel();
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
				.ConfigureAwait(false);
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
						var vmsToRemove = _forecast.Where(x => x.ArrivesInSeconds > 0).ToList();
						foreach(var vm in vmsToRemove)
						{
							_forecast.Remove(vm);
						}

						foreach (var forecastItem in forecast.Items) 
						{
							var duplicatedForecastItem = _forecast.FirstOrDefault(x => x.RouteStopId.Equals(forecastItem.RouteStop.Id));
							if (duplicatedForecastItem != null)
							{
								duplicatedForecastItem.ResetArrivedTime.Execute();
							}
							else
							{
								var vm = this.CreateFromForecastItem(forecastItem);
								if (vm != null)
								{
									_forecast.Add(vm);
								}
							}
						}

						vmsToRemove = _forecast.Where(x => x.ArrivedSeconds > StopLengthInSeconds).ToList();
						foreach(var vm in vmsToRemove)
						{
							_forecast.Remove(vm);
						}

						var nextStop = this.Forecast.FirstOrDefault(x => !x.IsCurrentRouteStop);
						if (nextStop != null)
						{
							nextStop.IsNextRouteStop = true;
						}

						this.NextStopForecast = this.Forecast.FirstOrDefault();
						_stateMachine.Fire(RouteVehicleVMTriggers.ForecastReturned);
					}
				});
			} 
			catch (Exception e)
			{
				Insights.Report(e, Xamarin.Insights.Severity.Warning);
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

					var currentStop = this.Forecast.LastOrDefault(x => x.IsCurrentRouteStop);
					if (currentStop != null)
					{
						var index = this.Forecast.IndexOf(currentStop) + 1;
						if (index < this.Forecast.Count)
						{
							this.Forecast[index].IsNextRouteStop = true;
						}
					}

					if (this.Forecast.Count(x => x.ArrivedSeconds > StopLengthInSeconds) > 0)
					{
						Task.Factory.StartNew(() => _stateMachine.Fire(RouteVehicleVMTriggers.ForecastRequested));
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
			vm.VehicleType = _route.VehicleTypes.First();
			vm.UpdateFromForecastItem(forecastItem);

			return vm;
		}

		private void RunUpdates(CancellationToken cancellationToken)
		{
			while(!cancellationToken.IsCancellationRequested)
			{
				try
				{
					Task.Delay(TimeSpan.FromSeconds(1), cancellationToken).Wait();
					this.Dispatcher.RequestMainThreadAction(() => this.CountdownCommand.Execute());
				} 
				catch (Exception e)
				{
					Insights.Report(e, Insights.Severity.Warning);
				}
			}
		}

		private void ShowOnMap()
		{
			_messenger.Publish<ShowVehicleForecastOnMapMessage>(new ShowVehicleForecastOnMapMessage(this, this.Vehicle.Id));
		}
	}
}