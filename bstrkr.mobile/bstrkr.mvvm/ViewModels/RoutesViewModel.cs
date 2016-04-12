using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using bstrkr.core;
using bstrkr.mvvm.navigation;
using bstrkr.providers;

using Cirrious.MvvmCross.ViewModels;

using Newtonsoft.Json;

using Xamarin;

namespace bstrkr.mvvm.viewmodels
{
    public class RoutesViewModel : BusTrackerViewModelBase
    {
        private readonly ILiveDataProviderFactory _providerFactory;
        private readonly ObservableCollection<RoutesListItemViewModel> _routes = new ObservableCollection<RoutesListItemViewModel>();

        private string _areaId;
        private bool _unknownArea;
        private RoutesListItemViewModel _selectedRoute;

        public RoutesViewModel(ILiveDataProviderFactory providerFactory)
        {
            _providerFactory = providerFactory;

            this.Routes = new ReadOnlyObservableCollection<RoutesListItemViewModel>(_routes);
            this.RefreshCommand = new MvxCommand<bool>(this.Refresh);
            this.ShowRouteVehiclesCommand = new MvxCommand<RoutesListItemViewModel>(this.ShowRouteDetails, vm => !this.IsBusy);
        }

        public MvxCommand<bool> RefreshCommand { get; private set; }

        public MvxCommand<RoutesListItemViewModel> ShowRouteVehiclesCommand { get; private set; }

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

        public ReadOnlyObservableCollection<RoutesListItemViewModel> Routes { get; private set; }

        public RoutesListItemViewModel SelectedRoute
        {
            get { return _selectedRoute; }
            set { this.RaiseAndSetIfChanged(ref _selectedRoute, value, () => this.SelectedRoute); }
        }

        protected override void OnIsBusyChanged()
        {
            base.OnIsBusyChanged();
            this.RefreshCommand.RaiseCanExecuteChanged();
        }

        private void Refresh(bool noCache = false)
        {
            var provider = _providerFactory.GetCurrentProvider();
            if (provider == null)
            {
                _areaId = string.Empty;
                _routes.Clear();
                this.UnknownArea = true;

                return;
            }

            if (provider.Area.Id.Equals(_areaId) && !noCache)
            {
                this.UpdateSelectedRoute();
                return;
            }

            _routes.Clear();
            _areaId = provider.Area.Id;
            this.UnknownArea = false;
            this.IsBusy = true;

            provider.GetRoutesAsync(noCache).ContinueWith(task =>
            {
                try
                {
                    if (task.Status == TaskStatus.RanToCompletion && task.Result != null)
                    {
                        this.Dispatcher.RequestMainThreadAction(() =>
                        {
                            this.CreateViewModels(task.Result);
                            this.UpdateSelectedRoute();
                        });
                    }
                }
                catch (Exception e)
                {
                    Insights.Report(e);
                }
                finally
                {
                    this.Dispatcher.RequestMainThreadAction(() => this.IsBusy = false);
                }
            }).ConfigureAwait(false);
        }

        private void CreateViewModels(IEnumerable<Route> routes)
        {
            foreach (var routeGroup in routes.GroupBy(r => new { r.Number, r.VehicleType }))
            {
                var firstRoute = routeGroup.First();
                _routes.Add(new RoutesListItemViewModel(
                                    routeGroup.ToList(),
                                    firstRoute.Name,
                                    routeGroup.Key.Number,
                                    routeGroup.Key.VehicleType));
            }
        }

        private void UpdateSelectedRoute()
        {
            if (this.SelectedRoute != null)
            {
                this.SelectedRoute = _routes.FirstOrDefault(r => r.Id.Equals(this.SelectedRoute.Id));
            }
        }

        private void ShowRouteDetails(RoutesListItemViewModel routeVM)
        {
            if (routeVM.Routes.Count > 1)
            {
                var routeListNavParam = new RouteListNavParam();
                foreach (var route in routeVM.Routes)
                {
                    routeListNavParam.Routes.Add(
                                new RouteListItem(
                                            route.Id,
                                            route.Name,
                                            route.Number,
                                            $"{route.FirstStop.Name} — {route.LastStop.Name}",
                                            route.VehicleType));
                }

                this.ShowViewModel<SetRouteViewModel>(new { routes = JsonConvert.SerializeObject(routeListNavParam) });
            }
            else
            {
                this.ShowViewModel<RouteVehiclesViewModel>(new
                {
                    routeId = routeVM.Routes.First().Id,
                    routeName = routeVM.Name,
                    routeNumber = routeVM.Routes.First().Number,
                    vehicleType = routeVM.VehicleType
                });
            }
        }
    }
}