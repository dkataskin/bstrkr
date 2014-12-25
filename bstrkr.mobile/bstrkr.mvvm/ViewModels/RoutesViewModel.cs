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
		private readonly ObservableCollection<UmbrellaRoutesListItemViewModel> _routes = new ObservableCollection<UmbrellaRoutesListItemViewModel>();

		private bool _unknownArea;

		public RoutesViewModel(IBusTrackerLocationService locationService, ILiveDataProviderFactory providerFactory)
		{
			_locationService = locationService;
			_liveDataProvider = providerFactory.CreateProvider(locationService.Area);

			this.Routes = new ReadOnlyObservableCollection<UmbrellaRoutesListItemViewModel>(_routes);
			this.RefreshCommand = new MvxCommand(this.Refresh);
			this.ShowRouteDetailsCommand = new MvxCommand<UmbrellaRoutesListItemViewModel>(this.ShowRouteDetails, vm => !this.IsBusy);
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

		public ReadOnlyObservableCollection<UmbrellaRoutesListItemViewModel> Routes { get; private set; }

		public MvxCommand RefreshCommand { get; private set; }

		public MvxCommand<UmbrellaRoutesListItemViewModel> ShowRouteDetailsCommand { get; private set; }

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
									_routes.Add(new UmbrellaRoutesListItemViewModel(
										routeGroup.Key,
										routeGroup.ToList()));
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

		private void ShowRouteDetails(UmbrellaRoutesListItemViewModel route)
		{
			this.ShowViewModel<UmbrellaRouteViewModel>(new 
			{ 
				name = route.Name, 
				routes = route.Routes.Select(x => x.Id).ToList() 
			});
		}

		public override void Start()
		{
			base.Start();
			this.RefreshCommand.Execute();
		}
	}
}