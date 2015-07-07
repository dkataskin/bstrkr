using System;
using System.Threading.Tasks;

using bstrkr.core.services.location;

using Cirrious.MvvmCross.ViewModels;

namespace bstrkr.mvvm.viewmodels
{
	public class LocationViewModel : BusTrackerViewModelBase
	{
		private readonly ILocationService _locationService;
		private int _timeoutInSeconds = 30;
		private Task _timeoutTask;

		public LocationViewModel(ILocationService locationService)
		{
			_locationService = locationService;
			_locationService.LocationUpdated += OnLocationUpdated;
			_locationService.LocatingFailed += OnLocationFailed;
		}

		public MvxCommand CancelCommand { get; private set; }

		public int TimeoutInSeconds
		{
			get { return _timeoutInSeconds; }
			private set { this.RaiseAndSetIfChanged(ref _timeoutInSeconds, value, () => this.TimeoutInSeconds); }
		}

		public override void Start()
		{
			base.Start();
			this.Locate();
		}

		private void Locate()
		{
			_locationService.StartUpdating();
			_timeoutTask = Task.Factory.StartNew(this.Countdown);
		}

		private void OnLocationFailed(object sender, LocationErrorEventArgs args)
		{
		}

		private void OnLocationUpdated(object sender, LocationUpdatedEventArgs args)
		{
		}

		private void Countdown()
		{
			while(this.TimeoutInSeconds > 0)
			{
				Task.Delay(1000).Wait();
				this.TimeoutInSeconds--;
			}
		}
	}
}