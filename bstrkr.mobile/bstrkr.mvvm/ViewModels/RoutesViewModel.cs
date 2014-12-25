using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Cirrious.MvvmCross.ViewModels;

using Xamarin;

using bstrkr.core;
using bstrkr.core.services.location;
using bstrkr.providers;

namespace bstrkr.mvvm.viewmodels
{
	public class RoutesViewModel : BusTrackerViewModelBase
	{
		private readonly IBusTrackerLocationService _locationService;
		private readonly ILiveDataProvider _liveDataProvider;
		private readonly ObservableCollection<UmbrellaRouteViewModel> _routes = new ObservableCollection<UmbrellaRouteViewModel>();

		private bool _unknownArea;

		public RoutesViewModel(IBusTrackerLocationService locationService, ILiveDataProviderFactory providerFactory)
		{
			_locationService = locationService;
			_liveDataProvider = providerFactory.CreateProvider(locationService.Area);

			this.Routes = new ReadOnlyObservableCollection<UmbrellaRouteViewModel>(_routes);
			this.RefreshCommand = new MvxCommand(this.Refresh);
			this.ShowRouteDetailsCommand = new MvxCommand<UmbrellaRouteViewModel>(this.ShowRouteDetails, vm => !this.IsBusy);
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

		public ReadOnlyObservableCollection<UmbrellaRouteViewModel> Routes { get; private set; }

		public MvxCommand RefreshCommand { get; private set; }

		public MvxCommand<UmbrellaRouteViewModel> ShowRouteDetailsCommand { get; private set; }

		private IEnumerable<RouteViewModel> CreateRouteViewModels(string name, Route route)
		{
			var vms = new List<RouteViewModel>();
			foreach (var vehicleType in route.VehicleTypes)
			{
				var vm = new RouteViewModel 
				{
					Id = route.Id,
					Name = name,
					VehicleType = vehicleType
				};

				if (route.FirstStop != null)
				{
					vm.From = route.FirstStop.Name;
				}

				if (route.LastStop != null)
				{
					vm.To = route.LastStop.Name;
				}

				vms.Add(vm);
			}

			return vms;
		}

		private void Refresh()
		{
			_routes.Clear();
			this.UnknownArea = _locationService.Area == null;

			if (!this.UnknownArea && !this.IsBusy)
			{
				this.IsBusy = true;

				_liveDataProvider.GetRoutesAsync().ContinueWith(task =>
				{
					try 
					{
						this.Dispatcher.RequestMainThreadAction(() =>
						{
							if (task.Result != null)
							{
								foreach (var routeGroup in task.Result.GroupBy(x => x.Number)) 
								{
									_routes.Add(new UmbrellaRouteViewModel(
										routeGroup.Key,
										routeGroup.SelectMany(x => this.CreateRouteViewModels(routeGroup.Key, x))
										.ToList()));
								}
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

		private void ShowRouteDetails(UmbrellaRouteViewModel route)
		{
		}

		public override void Start()
		{
			base.Start();
			this.RefreshCommand.Execute();
		}
	}
}