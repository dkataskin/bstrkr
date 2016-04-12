using bstrkr.core;

using Cirrious.MvvmCross.ViewModels;

namespace bstrkr.mvvm.viewmodels
{
    public class RouteStopForecastViewModel : BusTrackerViewModelBase
    {
        private int _arrivesInSeconds;
        private string _currentlyAt;

        public RouteStopForecastViewModel()
        {
            this.CountdownCommand = new MvxCommand(this.Countdown, () => this.ArrivesInSeconds > 0);
        }

        public MvxCommand CountdownCommand { get; private set; }

        public string VehicleId { get; set; }

        public VehicleTypes VehicleType { get; set; }

        public bool IsActive => this.ArrivesInSeconds > 0;

        public int ArrivesInSeconds
        {
            get { return _arrivesInSeconds; }
            set { this.RaiseAndSetIfChanged(ref _arrivesInSeconds, value, () => this.ArrivesInSeconds); }
        }

        public string CurrentlyAt
        {
            get { return _currentlyAt; }
            set { this.RaiseAndSetIfChanged(ref _currentlyAt, value, () => this.CurrentlyAt); }
        }

        public string LastStop { get; set; }

        public string RouteDisplayName { get; set; }

        public Route Route { get; set; }

        private void Countdown()
        {
            var isActive = this.IsActive;

            this.ArrivesInSeconds--;

            if (isActive != this.IsActive)
            {
                this.RaisePropertyChanged(() => this.IsActive);
            }
        }
    }
}