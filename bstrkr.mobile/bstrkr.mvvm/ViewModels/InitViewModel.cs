using System;
using System.Threading;
using System.Threading.Tasks;

using bstrkr.core.services.location;

using Cirrious.MvvmCross.ViewModels;

namespace bstrkr.mvvm.viewmodels
{
	public class InitViewModel : BusTrackerViewModelBase
	{
		private readonly IBusTrackerLocationService _locationService;

		private int _locatingSec = 30;

		private CancellationTokenSource _tokenSource = new CancellationTokenSource();
		private CancellationToken _token;

		public InitViewModel(IBusTrackerLocationService locationService)
		{
			_locationService = locationService;
			_locationService.LocationChanged += this.OnLocationChanged;
			_locationService.LocationError += this.OnLocationError;
		}

		public int LocatingSec
		{
			get { return _locatingSec; }
			private set { this.RaiseAndSetIfChanged(ref _locatingSec, value, () => this.LocatingSec); }
		}

		public override void Start()
		{
			base.Start();

			_token = _tokenSource.Token;
			this.Countdown(_token);

			//this.Dispatcher.RequestMainThreadAction(() => _locationService.Start());
		}

		private void Countdown(CancellationToken token)
		{
			Task.Factory.StartNew(() =>
			{
				while(!token.IsCancellationRequested && this.LocatingSec > 0)
				{
					Task.Delay(1000).Wait();
					this.LocatingSec--;
				}

				if (!token.IsCancellationRequested)
				{
					this.Dispatcher.RequestMainThreadAction(() => this.OnLocationError(this, LocationErrorEventArgs.Empty));
				}
			}, token);
		}

		private void OnLocationChanged(object sender, EventArgs args)
		{
			_tokenSource.Cancel();
		}

		private void OnLocationError(object sender, EventArgs args)
		{
			_tokenSource.Cancel();
		}
	}
}