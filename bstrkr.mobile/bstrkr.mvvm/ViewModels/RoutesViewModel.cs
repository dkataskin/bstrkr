using System;
using System.Collections.ObjectModel;

using Xamarin;

using bstrkr.core.services.location;
using bstrkr.providers;

namespace bstrkr.mvvm.viewmodels
{
	public class RoutesViewModel : BusTrackerViewModelBase
	{
		private readonly ObservableCollection<RouteViewModel> _routes = new ObservableCollection<RouteViewModel>();

		private bool _unknownArea;

		public RoutesViewModel(IBusTrackerLocationService locationService, ILiveDataProviderFactory providerFactory)
		{
			this.Routes = new ReadOnlyObservableCollection<RouteViewModel>(_routes);
			this.UnknownArea = locationService.Area != null;

			if (locationService.Area != null)
			{
				this.IsBusy = true;

				var provider = providerFactory.CreateProvider(locationService.Area);

				provider.GetRoutesAsync().ContinueWith(task =>
				{
					try 
					{
						this.Dispatcher.RequestMainThreadAction(() =>
						{
							foreach (var route in task.Result) 
							{
								_routes.Add(new RouteViewModel 
								{ 
									Name = route.Name,
									From = route.FirstStop.Name,
									To = route.LastStop.Name
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

		public ReadOnlyObservableCollection<RouteViewModel> Routes { get; private set; }
	}
}