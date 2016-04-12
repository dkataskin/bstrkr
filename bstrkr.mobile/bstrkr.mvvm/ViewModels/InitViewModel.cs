using System;
using System.Threading;
using System.Threading.Tasks;

using bstrkr.core;
using bstrkr.core.services.location;

using Chance.MvvmCross.Plugins.UserInteraction;

using Cirrious.MvvmCross.ViewModels;

namespace bstrkr.mvvm.viewmodels
{
    public class InitViewModel : BusTrackerViewModelBase
    {
        private readonly object _lockObject = new object();
        private readonly IAreaPositioningService _areaPositioningService;
        private readonly IUserInteraction _userInteraction;

        private int _locatingSec = 30;

        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();
        private CancellationToken _token;

        public InitViewModel(
                        IAreaPositioningService areaPositioningService,
                        IUserInteraction userInteraction)
        {
            _userInteraction = userInteraction;

            _areaPositioningService = areaPositioningService;
            _areaPositioningService.AreaLocated += this.OnAreaLocated;
            _areaPositioningService.AreaLocatingFailed += this.OnAreaLocatingFailed;

            this.SelectManuallyCommand = new MvxCommand(this.SelectManually);
            this.DetectLocationCommand = new MvxCommand(this.DetectLocation);
        }

        public MvxCommand SelectManuallyCommand { get; private set; }

        public MvxCommand DetectLocationCommand { get; private set; }

        public int LocatingSec
        {
            get { return _locatingSec; }
            private set { this.RaiseAndSetIfChanged(ref _locatingSec, value, () => this.LocatingSec); }
        }

        private void DetectLocation()
        {
            _token = _tokenSource.Token;
            this.Countdown(_token);

            this.Dispatcher.RequestMainThreadAction(() => _areaPositioningService.Start());
        }

        private void Countdown(CancellationToken token)
        {
            Task.Factory.StartNew(() =>
            {
                while (!token.IsCancellationRequested && this.LocatingSec > 0)
                {
                    Task.Delay(1000, token).Wait(token);
                    this.LocatingSec--;
                }

                if (!token.IsCancellationRequested)
                {
                    this.Dispatcher.RequestMainThreadAction(() => this.OnAreaLocatingFailed(this, LocationErrorEventArgs.Empty));
                }
            },
            token);
        }

        private void OnAreaLocated(object sender, EventArgs args)
        {
            lock (_lockObject)
            {
                this.CleanUp();
                this.ShowViewModel<HomeViewModel>();
            }
        }

        private void OnAreaLocatingFailed(object sender, EventArgs args)
        {
            lock (_lockObject)
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
            lock (_lockObject)
            {
                this.CleanUp();
                this.ShowViewModel<SetAreaViewModel>();
            }
        }

        private void CleanUp()
        {
            this.IsBusy = false;

            _tokenSource.Cancel();

            _areaPositioningService.AreaLocated -= this.OnAreaLocated;
            _areaPositioningService.AreaLocatingFailed -= this.OnAreaLocatingFailed;
        }
    }
}