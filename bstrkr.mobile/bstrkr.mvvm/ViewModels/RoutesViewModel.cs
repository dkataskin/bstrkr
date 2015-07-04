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
		private readonly ILiveDataProviderFactory _providerFactory;
		private readonly ObservableCollection<RoutesListItemViewModel> _routes = new ObservableCollection<RoutesListItemViewModel>();

		private bool _unknownArea;

		public RoutesViewModel(ILiveDataProviderFactory providerFactory)
		{
			_providerFactory = providerFactory;

			this.Routes = new ReadOnlyObservableCollection<RoutesListItemViewModel>(_routes);
			this.RefreshCommand = new MvxCommand(this.Refresh);
			this.ShowRouteVehiclesCommand = new MvxCommand<RoutesListItemViewModel>(this.ShowRouteDetails, vm => !this.IsBusy);
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

		public ReadOnlyObservableCollection<RoutesListItemViewModel> Routes { get; private set; }

		public MvxCommand RefreshCommand { get; private set; }

		public MvxCommand<RoutesListItemViewModel> ShowRouteVehiclesCommand { get; private set; }

		public override void Start()
		{
			base.Start();
			this.RefreshCommand.Execute();
		}

		protected override void OnIsBusyChanged()
		{
			base.OnIsBusyChanged();
			this.RefreshCommand.RaiseCanExecuteChanged();
		}

		private void Refresh()
		{
			_routes.Clear();
			var provider = _providerFactory.GetCurrentProvider();
			if (provider == null)
			{
				this.UnknownArea = true;
				return;
			}

			if (!this.IsBusy)
			{
				this.UnknownArea = false;
				this.IsBusy = true;

				provider.GetRoutesAsync().ContinueWith(task =>
				{
					try 
					{
						this.Dispatcher.RequestMainThreadAction(() =>
						{
							if (task.Result != null)
							{
								foreach (var route in task.Result) 
								{
									_routes.Add(new RoutesListItemViewModel
									{
										Id = route.Id,
										Name = route.Name,
										VehicleType = route.VehicleTypes.First(),
										Route = route
									});
								}
							}
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
				}).ConfigureAwait(false);
			}
		}

		private void ShowRouteDetails(RoutesListItemViewModel routeVM)
		{
			var ids = string.Join(",", routeVM.Route.Ids);

			this.ShowViewModel<RouteVehiclesViewModel>(new 
			{ 
				routeId = routeVM.Id, 
				routeName = routeVM.Name,
				routeNumber = routeVM.Route.Number,
				routeIds = ids,
				vehicleType = routeVM.Route.VehicleTypes.First()
			});
		}
	}
}