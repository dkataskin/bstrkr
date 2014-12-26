using System;
using System.Collections.ObjectModel;

using Cirrious.MvvmCross.ViewModels;

using Xamarin;

using bstrkr.core.services.location;
using bstrkr.providers;

namespace bstrkr.mvvm.viewmodels
{
	public class RouteStopsViewModel : BusTrackerViewModelBase
	{
		private readonly ILiveDataProviderFactory _providerFactory;
		private readonly ObservableCollection<RouteStopViewModel> _stops = new ObservableCollection<RouteStopViewModel>();

		private bool _unknownArea;

		public RouteStopsViewModel(ILiveDataProviderFactory providerFactory)
		{
			_providerFactory = providerFactory;
			this.Stops = new ReadOnlyObservableCollection<RouteStopViewModel>(_stops);

			this.RefreshCommand = new MvxCommand(this.Refresh, () => !this.IsBusy);
		}

		public bool UnknownArea
		{
			get 
			{
				return _unknownArea;
			}

			set
			{
				if (_unknownArea != value)
				{
					_unknownArea = value;
					this.RaisePropertyChanged(() => this.UnknownArea);
				}
			}
		}

		public ReadOnlyObservableCollection<RouteStopViewModel> Stops { get; private set; }

		public MvxCommand RefreshCommand { get; private set; }

		public override void Start()
		{
			base.Start();
			this.Refresh();
		}

		protected override void OnIsBusyChanged()
		{
			base.OnIsBusyChanged();
			this.RefreshCommand.RaiseCanExecuteChanged();
		}

		private void Refresh()
		{
			_stops.Clear();

			var provider = _providerFactory.GetCurrentProvider();
			if (provider == null)
			{
				this.UnknownArea = true;
			}
			else
			{
				this.UnknownArea = false;
				this.IsBusy = true;

				provider.GetRouteStopsAsync().ContinueWith(task =>
				{
					try 
					{
						this.Dispatcher.RequestMainThreadAction(() =>
						{
							foreach (var stop in task.Result) 
							{
								_stops.Add(new RouteStopViewModel 
								{ 
									Name = stop.Name
								});
							}
						});
					} 
					catch (Exception e) 
					{
						Insights.Report(e, ReportSeverity.Warning);
					}
					finally
					{
						this.Dispatcher.RequestMainThreadAction(() => this.IsBusy = false);
					}
				});
			}
		}
	}
}