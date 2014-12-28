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
		private readonly ObservableCollection<UmbrellaRoutesListItemViewModel> _routes = new ObservableCollection<UmbrellaRoutesListItemViewModel>();

		private bool _unknownArea;

		public RoutesViewModel(ILiveDataProviderFactory providerFactory)
		{
			_providerFactory = providerFactory;

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

		public string Title 
		{
			get { return AppResources.routes_view_title; }
		}

		public ReadOnlyObservableCollection<UmbrellaRoutesListItemViewModel> Routes { get; private set; }

		public MvxCommand RefreshCommand { get; private set; }

		public MvxCommand<UmbrellaRoutesListItemViewModel> ShowRouteDetailsCommand { get; private set; }

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
			}

			if (!this.UnknownArea && !this.IsBusy)
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
				routes = string.Join(",", route.Routes.Select(x => x.Id))
			});
		}
	}
}