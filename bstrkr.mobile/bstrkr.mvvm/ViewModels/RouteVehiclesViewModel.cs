﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

using bstrkr.core;
using bstrkr.providers;

using Chance.MvvmCross.Plugins.UserInteraction;

using Cirrious.CrossCore;
using Cirrious.CrossCore.Platform;
using Cirrious.MvvmCross.ViewModels;

using Xamarin;

namespace bstrkr.mvvm.viewmodels
{
    public class RouteVehiclesViewModel : BusTrackerViewModelBase, ICleanable
    {
        private readonly object _lockObject = new object();
        private readonly ILiveDataProviderFactory _providerFactory;
        private readonly IList<VehicleForecastViewModel> _allVehicles = new List<VehicleForecastViewModel>();
        private readonly ObservableCollection<VehicleForecastViewModel> _vehicles = new ObservableCollection<VehicleForecastViewModel>();

        private readonly IObservable<long> _intervalObservable;
        private readonly IDisposable _intervalSubscription;
        private readonly IUserInteraction _userInteraction;

        private bool _noData;
        private string _routeId;
        private string _name;
        private int _number;
        private VehicleTypes _vehicleType;
        private Route _route;

        public RouteVehiclesViewModel(ILiveDataProviderFactory providerFactory, IUserInteraction userInteraction)
        {
            _providerFactory = providerFactory;
            _userInteraction = userInteraction;

            this.Vehicles = new ReadOnlyObservableCollection<VehicleForecastViewModel>(_vehicles);
            this.ShowVehicleOnMapCommand = new MvxCommand<VehicleForecastViewModel>(this.ShowVehicleOnMap, vm => !this.IsBusy);

            _intervalObservable = Observable.Interval(TimeSpan.FromMilliseconds(1000));
            _intervalSubscription = _intervalObservable.Subscribe(this.OnNextInterval);
        }

        public MvxCommand<VehicleForecastViewModel> ShowVehicleOnMapCommand { get; private set; }

        public string RouteId
        {
            get { return _routeId; }
            private set { this.RaiseAndSetIfChanged(ref _routeId, value, () => this.RouteId); }
        }

        public int Number
        {
            get { return _number; }
            private set { this.RaiseAndSetIfChanged(ref _number, value, () => this.Number); }
        }

        public string Name
        {
            get { return _name; }
            private set { this.RaiseAndSetIfChanged(ref _name, value, () => this.Name); }
        }

        public bool NoData
        {
            get { return _noData; }
            private set { this.RaiseAndSetIfChanged(ref _noData, value, () => this.NoData); }
        }

        public VehicleTypes VehicleType
        {
            get { return _vehicleType; }
            private set { this.RaiseAndSetIfChanged(ref _vehicleType, value, () => this.VehicleType); }
        }

        public Route Route
        {
            get { return _route; }
            private set { this.RaiseAndSetIfChanged(ref _route, value, () => this.Route); }
        }

        public ReadOnlyObservableCollection<VehicleForecastViewModel> Vehicles { get; private set; }

        public ReadOnlyObservableCollection<RouteStop> Stops { get; private set; }

        public void Init(
                    string routeId,
                    string routeName,
                    int routeNumber,
                    VehicleTypes vehicleType)
        {
            this.RouteId = routeId;
            this.Number = routeNumber;
            this.Name = routeName;
            this.VehicleType = vehicleType;
        }

        public override void Start()
        {
            base.Start();

            this.IsBusy = true;

            var provider = _providerFactory.GetCurrentProvider();
            if (provider != null)
            {
                provider.GetRouteAsync(this.RouteId)
                        .ContinueWith(getRouteTask =>
                        {
                            if (getRouteTask.Status == TaskStatus.RanToCompletion && getRouteTask.Result != null)
                            {
                                this.Route = getRouteTask.Result;
                                provider.GetRouteVehiclesAsync(new[] { this.Route }).ContinueWith(this.ShowRouteVehicles);
                            }
                            else
                            {
                                this.Route = null;
                                this.NoData = true;
                                this.IsBusy = false;
                            }
                        });
            }
        }

        public void CleanUp()
        {
            if (_intervalSubscription != null)
            {
                _intervalSubscription.Dispose();
                foreach (var vm in _allVehicles)
                {
                    vm.PropertyChanged -= this.OnRouteVehicleVMPropertyChanged;
                    vm.CleanUp();
                }

                _allVehicles.Clear();
                _vehicles.Clear();
            }
        }

        private void SetRoute(Task<Route> task)
        {
            if (task.Status == TaskStatus.RanToCompletion && task.Result != null)
            {
                this.Route = task.Result;
            }
        }

        private void ShowRouteVehicles(Task<IEnumerable<Vehicle>> task)
        {
            if (task.Status != TaskStatus.RanToCompletion)
            {
                Insights.Report(task.Exception, Insights.Severity.Error);
                return;
            }

            var vehicles = task.Result;
            if (vehicles != null && vehicles.Any())
            {
                var vms = vehicles.Select(this.CreateFromVehicle).ToList();
                this.Dispatcher.RequestMainThreadAction(() =>
                {
                    lock (_lockObject)
                    {
                        foreach (var vm in vms)
                        {
                            _allVehicles.Add(vm);
                            vm.PropertyChanged += this.OnRouteVehicleVMPropertyChanged;
                        }
                    }

                    this.IsBusy = false;

                    this.UpdateForecastAsync()
                        .ContinueWith(task1 => MvxTrace.Trace("Finished updating forecasts"))
                        .ConfigureAwait(false);
                });
            }
            else
            {
                this.Dispatcher.RequestMainThreadAction(() =>
                {
                    this.NoData = true;
                    this.IsBusy = false;
                });
            }
        }

        private VehicleForecastViewModel CreateFromVehicle(Vehicle vehicle)
        {
            var vm = Mvx.IocConstruct<VehicleForecastViewModel>();
            vm.InitWithData(vehicle, this.Route, false);

            return vm;
        }

        private void OnNextInterval(long interval)
        {
            this.Dispatcher.RequestMainThreadAction(() =>
            {
                lock (_lockObject)
                {
                    foreach (var vm in _vehicles)
                    {
                        vm.CountdownCommand.Execute();
                    }
                }
            });
        }

        private async Task UpdateForecastAsync()
        {
            await Task.Factory.StartNew(() =>
            {
                MvxTrace.Trace("There are {0} vehicles in the list", _allVehicles.Count());
                foreach (var vehicle in _allVehicles)
                {
                    MvxTrace.Trace("Updating forecast for {0}", vehicle.Vehicle.CarPlate);
                    vehicle.UpdateForecastCommand.Execute();
                    MvxTrace.Trace("Forecast for {0} updated", vehicle.Vehicle.CarPlate);
                }
            });
        }

        private void OnRouteVehicleVMPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "State")
            {
                lock (_lockObject)
                {
                    var vm = sender as VehicleForecastViewModel;
                    if (vm.State == RouteVehicleVMStates.NoForecast)
                    {
                        _vehicles.Remove(vm);
                    }
                    else if (!_vehicles.Contains(vm))
                    {
                        _vehicles.Add(vm);
                    }
                }
            }
        }

        private void ShowVehicleOnMap(VehicleForecastViewModel vehicleViewModel)
        {
            _userInteraction.Confirm(
                    AppResources.show_vehicle_on_map_confirm_format,
                    () => vehicleViewModel.ShowOnMapCommand.Execute(),
                    string.Empty,
                    AppResources.yes,
                    AppResources.no);
        }
    }
}