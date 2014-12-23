using System;
using System.Collections.ObjectModel;

using Xamarin;

using bstrkr.core.services.location;
using bstrkr.providers;

namespace bstrkr.mvvm.viewmodels
{
	public class RouteStopsViewModel : BusTrackerViewModelBase
	{
		private readonly ObservableCollection<RouteStopViewModel> _stops = new ObservableCollection<RouteStopViewModel>();

		private bool _unknownArea;

		public RouteStopsViewModel(IBusTrackerLocationService locationService, ILiveDataProviderFactory providerFactory)
		{
			this.Stops = new ReadOnlyObservableCollection<RouteStopViewModel>(_stops);
			this.UnknownArea = locationService.Area == null;

			if (locationService.Area != null)
			{
				this.IsBusy = true;

				var provider = providerFactory.CreateProvider(locationService.Area);

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
	}
}