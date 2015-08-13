using System;
using System.Threading;
using System.Threading.Tasks;

using bstrkr.core;
using bstrkr.core.services.location;

using Chance.MvvmCross.Plugins.UserInteraction;

using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;

namespace bstrkr.mvvm.viewmodels
{
	public class InitViewModel : BusTrackerViewModelBase
	{
		private readonly object _lockObject = new object();
		private readonly IBusTrackerLocationService _locationService;
		private readonly IUserInteraction _userInteraction;

		private int _locatingSec = 30;

		private CancellationTokenSource _tokenSource = new CancellationTokenSource();
		private CancellationToken _token;

		public InitViewModel(IBusTrackerLocationService locationService,
							 IUserInteraction userInteraction)
		{
			_userInteraction = userInteraction;

			_locationService = locationService;
			_locationService.LocationChanged += this.OnLocationChanged;
			_locationService.LocationError += this.OnLocationError;

			this.SelectManuallyCommand = new MvxCommand(this.SelectManually);
		}

		public MvxCommand SelectManuallyCommand { get; private set; }

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

			this.Dispatcher.RequestMainThreadAction(() => _locationService.Start());
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
			}, 
			token);
		}

		private void OnLocationChanged(object sender, EventArgs args)
		{
			lock(_lockObject)
			{
				this.CleanUp();
				this.ShowViewModel<HomeViewModel>();
			}
		}

		private void OnLocationError(object sender, EventArgs args)
		{
			lock(_lockObject)
			{
				this.CleanUp();

				_userInteraction.Confirm(
					AppResources.unknown_location_dialog_text, 
					answer => 
					{
						if (answer)
						{
							this.ShowViewModel<SetAreaViewModel>();
						}
						else
						{
							this.ShowViewModel<HomeViewModel>();
						}
					},
					AppResources.unknown_location_dialog_title,
					AppResources.yes,
					AppResources.no_thanks);
			}
		}

		private void SelectManually()
		{
			lock(_lockObject)
			{
				this.CleanUp();
				this.ShowViewModel<SetAreaViewModel>();
			}
		}

		private void CleanUp()
		{
			this.IsBusy = false;

			_tokenSource.Cancel();

			_locationService.LocationChanged -= this.OnLocationChanged;
			_locationService.LocationError -= this.OnLocationError;
		}
	}
}