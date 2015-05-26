using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using bstrkr.core.services.location;
using bstrkr.mvvm.navigation;
using bstrkr.providers;

using Chance.MvvmCross.Plugins.UserInteraction;

using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;

using Newtonsoft.Json;

using Xamarin;
using System.Threading.Tasks;
using bstrkr.core;

namespace bstrkr.mvvm.viewmodels
{
	public class RouteStopsViewModel : BusTrackerViewModelBase
	{
		private const int RouteStopMaximumDistanceInMeters = 1200;

		private readonly object _lockObject = new object();
		private readonly ILiveDataProviderFactory _providerFactory;
		private readonly IBusTrackerLocationService _locationService;

		private readonly ObservableCollection<RouteStopsListItemViewModel> _closeStops = 
			new ObservableCollection<RouteStopsListItemViewModel>();

		private readonly ObservableCollection<RouteStopsListItemViewModel> _stops = 
			new ObservableCollection<RouteStopsListItemViewModel>();

		private IList<RouteStopsListItemViewModel> _allStops = new List<RouteStopsListItemViewModel>();
		private IList<RouteStopsListItemViewModel> _closeAllStops = new List<RouteStopsListItemViewModel>();

		private bool _unknownArea;
		private string _filterString;


		public RouteStopsViewModel(
						ILiveDataProviderFactory providerFactory,
						IBusTrackerLocationService locationService)
		{
			_providerFactory = providerFactory;
			_locationService = locationService;

			this.Stops = new ReadOnlyObservableCollection<RouteStopsListItemViewModel>(_stops);
			this.CloseStops = new ReadOnlyObservableCollection<RouteStopsListItemViewModel>(_closeStops);

			this.RefreshCommand = new MvxCommand(this.Refresh, () => !this.IsBusy);
			this.ShowStopDetailsCommand = new MvxCommand<RouteStopsListItemViewModel>(this.ShowStopDetails, vm => !this.IsBusy);
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

		public string FilterSting
		{
			get { return _filterString; }
			set 
			{ 
				if (!string.Equals(_filterString, value))
				{
					_filterString = value;
					this.RaisePropertyChanged(() => this.FilterSting);
					this.Filter(value);
				}
			}
		}

		public ReadOnlyObservableCollection<RouteStopsListItemViewModel> CloseStops { get; private set; }

		public ReadOnlyObservableCollection<RouteStopsListItemViewModel> Stops { get; private set; }

		public MvxCommand RefreshCommand { get; private set; }

		public MvxCommand<RouteStopsListItemViewModel> ShowStopDetailsCommand { get; private set; }

		public override void Start()
		{
			base.Start();
			this.Refresh();
		}

		protected override void OnIsBusyChanged()
		{
			base.OnIsBusyChanged();
			this.RefreshCommand.RaiseCanExecuteChanged();
			this.ShowStopDetailsCommand.RaiseCanExecuteChanged();
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

				provider.GetRouteStopsAsync()
						.ContinueWith(this.OnRouteStopListReceived)
						.ConfigureAwait(false);
			}
		}

		private void ShowStopDetails(RouteStopsListItemViewModel routeStopViewModel)
		{
			if (routeStopViewModel.Stops.Count > 1)
			{
				var routeStopListNavParam = new RouteStopListNavParam();
				foreach (var routeStop in routeStopViewModel.Stops)
				{
					routeStopListNavParam.RouteStops.Add(new RouteStopListItem(routeStop.Id, routeStop.Name, routeStop.Description));
				}

				this.ShowViewModel<SetRouteStopViewModel>(new { stops = JsonConvert.SerializeObject(routeStopListNavParam) });
			}
			else
			{
				var routeStop = routeStopViewModel.Stops.First();
				this.ShowViewModel<RouteStopViewModel>(new 
				{ 
					id = routeStop.Id,
					name = routeStop.Name,
					description = routeStop.Description
				});
			}
		}

		private void Filter(string filter)
		{
			lock (_lockObject)
			{
				Func<RouteStopsListItemViewModel, bool> nameFilterPredicate = 
					(RouteStopsListItemViewModel vm) => vm.Name.Contains(filter);

				if (string.IsNullOrEmpty(filter))
				{
					nameFilterPredicate = (RouteStopsListItemViewModel vm) => true;
				}

				this.ClearAndFilter(_stops, _allStops, nameFilterPredicate);
				this.ClearAndFilter(_closeStops, _closeAllStops, nameFilterPredicate);
			}
		}

		private void ClearAndFilter(
					ObservableCollection<RouteStopsListItemViewModel> observable,
					IEnumerable<RouteStopsListItemViewModel> source,
					Func<RouteStopsListItemViewModel, bool> filter)
		{
			observable.Clear();
			foreach (var vm in source.Where(filter))
			{
				observable.Add(vm);
			}
		}

		private void OnRouteStopListReceived(Task<IEnumerable<RouteStop>> getRouteStopsTask)
		{
			try 
			{
				this.Dispatcher.RequestMainThreadAction(() =>
				{
					lock(_lockObject)
					{
						var location = _locationService.Location;
						foreach (var stopsGroup in getRouteStopsTask.Result.GroupBy(x => x.Name)) 
						{
							var vm = new RouteStopsListItemViewModel(stopsGroup.Key, stopsGroup.ToList());
							vm.CalculateDistanceCommand.Execute(location);

							_stops.Add(vm);
						}

						_allStops = _stops.OrderBy(x => x.Name).ToList();
						_closeAllStops = _allStops.OrderBy(vm => vm.DistanceInMeters)
							.Where(vm => vm.DistanceInMeters <= RouteStopMaximumDistanceInMeters)
							.ToList();
					}

					this.Filter(this.FilterSting);
				});
			} 
			catch (Exception e) 
			{
				Insights.Report(e, Xamarin.Insights.Severity.Warning);
			}
			finally
			{
				this.Dispatcher.RequestMainThreadAction(() => this.IsBusy = false);
			}
		}
	}
}