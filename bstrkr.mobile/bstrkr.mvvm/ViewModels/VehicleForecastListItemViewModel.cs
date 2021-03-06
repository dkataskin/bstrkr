﻿using bstrkr.core;

using Cirrious.MvvmCross.ViewModels;

namespace bstrkr.mvvm.viewmodels
{
    public class VehicleForecastListItemViewModel : BusTrackerViewModelBase
    {
        private int _arrivesInSeconds;
        private int _arrivedSeconds;
        private string _routeStopId;
        private string _routeStopName;
        private string _routeStopDescription;
        private VehicleTypes _vehicleType;
        private bool _isNextRouteStop;

        public VehicleForecastListItemViewModel()
        {
            this.CountdownCommand = new MvxCommand(this.Countdown);
            this.ResetArrivedTime = new MvxCommand(this.Reset, () => this.IsCurrentRouteStop);
        }

        public MvxCommand CountdownCommand { get; private set; }

        public MvxCommand ResetArrivedTime { get; private set; }

        public bool IsCurrentRouteStop => this.ArrivesInSeconds == 0;

        public bool IsNextRouteStop
        {
            get { return _isNextRouteStop; }
            set { this.RaiseAndSetIfChanged(ref _isNextRouteStop, value, () => this.IsNextRouteStop); }
        }

        public int ArrivesInSeconds
        {
            get { return _arrivesInSeconds; }
            private set
            {
                this.RaiseAndSetIfChanged(ref _arrivesInSeconds, value, () => this.ArrivesInSeconds);
            }
        }

        public int ArrivedSeconds
        {
            get { return _arrivedSeconds; }
            private set
            {
                this.RaiseAndSetIfChanged(ref _arrivedSeconds, value, () => this.ArrivedSeconds);
            }
        }

        public VehicleTypes VehicleType
        {
            get { return _vehicleType; }
            set { this.RaiseAndSetIfChanged(ref _vehicleType, value, () => this.VehicleType); }
        }

        public string RouteStopId
        {
            get { return _routeStopId; }
            private set
            {
                this.RaiseAndSetIfChanged(ref _routeStopId, value, () => this.RouteStopId);
            }
        }

        public string RouteStopName
        {
            get { return _routeStopName; }
            private set
            {
                this.RaiseAndSetIfChanged(ref _routeStopName, value, () => this.RouteStopName);
            }
        }

        public string RouteStopDescription
        {
            get { return _routeStopDescription; }
            private set
            {
                this.RaiseAndSetIfChanged(ref _routeStopDescription, value, () => this.RouteStopDescription);
            }
        }

        public void UpdateFromForecastItem(VehicleForecastItem forecastItem)
        {
            this.ArrivesInSeconds = forecastItem.ArrivesInSec;
            if (forecastItem.RouteStop != null)
            {
                this.RouteStopId = forecastItem.RouteStop.Id;
                this.RouteStopName = forecastItem.RouteStop.Name;
                this.RouteStopDescription = forecastItem.RouteStop.Description;
            }
        }

        private void Countdown()
        {
            var raiseCurrentRouteStopChanged = false;
            if (this.ArrivesInSeconds > 0)
            {
                this.ArrivesInSeconds--;
                if (this.ArrivesInSeconds == 0)
                {
                    raiseCurrentRouteStopChanged = true;
                }
            }
            else
            {
                this.ArrivedSeconds++;
            }

            if (raiseCurrentRouteStopChanged)
            {
                this.RaisePropertyChanged(() => this.IsCurrentRouteStop);
            }
        }

        private void Reset()
        {
            this.ArrivedSeconds = 1;
        }
    }
}