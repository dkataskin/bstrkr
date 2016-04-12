using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using bstrkr.core;
using bstrkr.core.services.location;
using bstrkr.mvvm.navigation;
using bstrkr.providers;

using Cirrious.MvvmCross.ViewModels;

using Newtonsoft.Json;

using Xamarin;

namespace bstrkr.mvvm.viewmodels
{
    public class RouteStopsViewModel : BusTrackerViewModelBase
    {
        private readonly object _lockObject = new object();
        private readonly ILiveDataProviderFactory _providerFactory;
        private readonly IBusTrackerLocationService _locationService;

        private readonly ObservableCollection<RouteStopsListItemViewModel> _stops =
            new ObservableCollection<RouteStopsListItemViewModel>();

        private IList<RouteStopsListItemViewModel> _allStops = new List<RouteStopsListItemViewModel>();

        private string _areaId;
        private bool _unknownArea;
        private string _filterString;

        public RouteStopsViewModel(
                        ILiveDataProviderFactory providerFactory,
                        IBusTrackerLocationService locationService)
        {
            _providerFactory = providerFactory;
            _locationService = locationService;

            this.Stops = new ReadOnlyObservableCollection<RouteStopsListItemViewModel>(_stops);

            this.RefreshCommand = new MvxCommand<bool>(this.Refresh, noCache => !this.IsBusy);
            this.ShowStopDetailsCommand = new MvxCommand<RouteStopsListItemViewModel>(this.ShowStopDetails, vm => !this.IsBusy);
        }

        public bool UnknownArea
        {
            get { return _unknownArea; }
            set { this.RaiseAndSetIfChanged(ref _unknownArea, value, () => this.UnknownArea); }
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

        public ReadOnlyObservableCollection<RouteStopsListItemViewModel> Stops { get; private set; }

        public MvxCommand<bool> RefreshCommand { get; private set; }

        public MvxCommand<RouteStopsListItemViewModel> ShowStopDetailsCommand { get; private set; }

        protected override void OnIsBusyChanged()
        {
            base.OnIsBusyChanged();
            this.RefreshCommand.RaiseCanExecuteChanged();
            this.ShowStopDetailsCommand.RaiseCanExecuteChanged();
        }

        private void Refresh(bool noCache = false)
        {
            var provider = _providerFactory.GetCurrentProvider();
            if (provider == null)
            {
                _areaId = string.Empty;
                _stops.Clear();
                this.UnknownArea = true;
                return;
            }

            if (provider.Area.Id.Equals(_areaId) && !noCache)
            {
                return;
            }

            _allStops.Clear();
            _stops.Clear();
            _areaId = provider.Area.Id;
            this.UnknownArea = false;
            this.IsBusy = true;

            provider.GetRouteStopsAsync()
                    .ContinueWith(this.OnRouteStopListReceived)
                    .ConfigureAwait(false);
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
                    (RouteStopsListItemViewModel vm) => vm.Name.ToLowerInvariant().Contains(filter.ToLowerInvariant());

                if (string.IsNullOrEmpty(filter))
                {
                    nameFilterPredicate = (RouteStopsListItemViewModel vm) => true;
                }

                this.ClearAndFilter(_stops, _allStops, nameFilterPredicate);
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
                    lock (_lockObject)
                    {
                        var location = _locationService.GetLastLocation();
                        foreach (var stopsGroup in getRouteStopsTask.Result.GroupBy(x => x.Name))
                        {
                            var routeStopList = stopsGroup.ToList();
                            var clusters = new List<List<RouteStop>>();

                            if (routeStopList.Count > 1)
                            {
                                var stopsGraphEdges = new List<RouteStopsGraphEdge>();
                                for (int i = 0; i < routeStopList.Count - 1; i++)
                                {
                                    for (int j = i + 1; j < routeStopList.Count; j++)
                                    {
                                        stopsGraphEdges.Add(new RouteStopsGraphEdge
                                        {
                                            FirstStop = routeStopList[i],
                                            SecondStop = routeStopList[j],
                                            Length = routeStopList[i].Location.Position.DistanceTo(routeStopList[j].Location.Position) * 1000
                                        });
                                    }
                                }

                                stopsGraphEdges = stopsGraphEdges.OrderBy(x => x.Length).ToList();
                                var shortestEdge = stopsGraphEdges.FirstOrDefault(x => x.Length <= 250.0d);
                                if (shortestEdge != null)
                                {
                                    clusters.Add(new List<RouteStop> { shortestEdge.FirstStop, shortestEdge.SecondStop });
                                    stopsGraphEdges.Remove(shortestEdge);
                                }
                                else
                                {
                                    clusters.Add(new List<RouteStop> { stopsGraphEdges.First().FirstStop });
                                    clusters.Add(new List<RouteStop> { stopsGraphEdges.First().SecondStop });
                                    stopsGraphEdges.RemoveAt(0);
                                }

                                foreach (var edge in stopsGraphEdges)
                                {
                                    var firstNodeCluster = clusters.FirstOrDefault(x => x.Contains(edge.FirstStop));
                                    var secondNodeCluster = clusters.FirstOrDefault(x => x.Contains(edge.SecondStop));

                                    if (firstNodeCluster == secondNodeCluster)
                                    {
                                        continue;
                                    }

                                    if (edge.Length <= 250.0d)
                                    {
                                        if (firstNodeCluster != null && secondNodeCluster != null)
                                        {
                                            clusters.Remove(secondNodeCluster);
                                            firstNodeCluster.AddRange(secondNodeCluster);
                                        }

                                        if (firstNodeCluster == null && secondNodeCluster == null)
                                        {
                                            clusters.Add(new List<RouteStop> { edge.FirstStop, edge.SecondStop });
                                        }

                                        firstNodeCluster?.Add(edge.SecondStop);

                                        secondNodeCluster?.Add(edge.FirstStop);
                                    }
                                    else
                                    {
                                        if (firstNodeCluster != null && secondNodeCluster != null)
                                        {
                                            continue;
                                        }

                                        if (firstNodeCluster == null && secondNodeCluster == null)
                                        {
                                            clusters.Add(new List<RouteStop> { edge.FirstStop });
                                            clusters.Add(new List<RouteStop> { edge.SecondStop });
                                        }

                                        if (firstNodeCluster != null)
                                        {
                                            clusters.Add(new List<RouteStop> { edge.SecondStop });
                                        }

                                        if (secondNodeCluster != null)
                                        {
                                            clusters.Add(new List<RouteStop> { edge.FirstStop });
                                        }
                                    }
                                }
                            }
                            else
                            {
                                clusters.Add(routeStopList);
                            }

                            foreach (var cluster in clusters)
                            {
                                var vm = new RouteStopsListItemViewModel(stopsGroup.Key, cluster);
                                vm.CalculateDistanceCommand.Execute(location);

                                _allStops.Add(vm);
                            }
                        }

                        _allStops = _allStops.OrderBy(vm => vm.DistanceInMeters).ToList();
                    }

                    this.Filter(this.FilterSting);
                });
            }
            catch (Exception e)
            {
                Insights.Report(e);
            }
            finally
            {
                this.Dispatcher.RequestMainThreadAction(() => this.IsBusy = false);
            }
        }

        private class RouteStopsGraphEdge
        {
            public RouteStop FirstStop { get; set; }

            public RouteStop SecondStop { get; set; }

            public double Length { get; set; }
        }
    }
}