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

namespace bstrkr.mvvm.viewmodels
{
	public class RouteStopsViewModel : BusTrackerViewModelBase
	{
		private const int ProximityFilteredStopListMaxCount = 10;

		private readonly ILiveDataProviderFactory _providerFactory;
		private readonly IBusTrackerLocationService _locationService;

		private readonly ObservableCollection<RouteStopsListItemViewModel> _stops = new ObservableCollection<RouteStopsListItemViewModel>();

		private bool _unknownArea;
		private bool _proximityFilter = true;
		private string _filterString;
		private IList<RouteStopsListItemViewModel> _allStops = new List<RouteStopsListItemViewModel>();

		public RouteStopsViewModel(
						ILiveDataProviderFactory providerFactory,
						IBusTrackerLocationService locationService)
		{
			_providerFactory = providerFactory;
			_locationService = locationService;

			this.Stops = new ReadOnlyObservableCollection<RouteStopsListItemViewModel>(_stops);

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

		public bool ProximityFilter
		{
			get { return _proximityFilter; }
			set
			{
				if (_proximityFilter != value)
				{
					_proximityFilter = value;
					this.RaisePropertyChanged(() => this.ProximityFilter);
					this.Filter(this.FilterSting, value);
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
					this.Filter(value, this.ProximityFilter);
				}
			}
		}

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

				provider.GetRouteStopsAsync().ContinueWith(task =>
				{
					try 
					{
						this.Dispatcher.RequestMainThreadAction(() =>
						{
							var location = _locationService.Location;
							foreach (var stopsGroup in task.Result.GroupBy(x => x.Name)) 
							{
								var vm = new RouteStopsListItemViewModel(stopsGroup.Key, stopsGroup.ToList());
								vm.CalculateDistanceCommand.Execute(location);

								_stops.Add(vm);
							}

							_allStops = _stops.OrderBy(x => x.Name).ToList();
							this.Filter(this.FilterSting, this.ProximityFilter);
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

		private void Filter(string filter, bool filterByProximity)
		{
			var stopsFilteredByName = new List<RouteStopsListItemViewModel>();
			if (string.IsNullOrEmpty(filter))
			{
				stopsFilteredByName = _allStops.ToList();
			}
			else
			{
				foreach (var vm in _allStops.Where(vm => vm.Name.Contains(filter)).ToList())
				{
					stopsFilteredByName.Add(vm);
				}
			}

			var stopsFilteredByProximity = stopsFilteredByName;
			if (filterByProximity)
			{
				var location = _locationService.Location;
				stopsFilteredByProximity = stopsFilteredByName
											.OrderBy(vm => vm.DistanceInMeters)
											.Take(ProximityFilteredStopListMaxCount)
											.ToList();
			}

			_stops.Clear();
			foreach (var vm in stopsFilteredByProximity)
			{
				_stops.Add(vm);
			}
		}
	}
}