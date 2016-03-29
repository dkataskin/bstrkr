using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

using bstrkr.core;
using bstrkr.core.config;
using bstrkr.core.services.location;
using bstrkr.mvvm.messages;

using Cirrious.CrossCore;
using Cirrious.MvvmCross.Plugins.Messenger;
using Cirrious.MvvmCross.ViewModels;

namespace bstrkr.mvvm.viewmodels
{
	public class HomeViewModel : BusTrackerViewModelBase
    {
		private readonly IBusTrackerLocationService _locationService;
		private readonly IMvxMessenger _messenger;
		private readonly MvxSubscriptionToken _taskChangedMessagesSubscription;
		private readonly IConfigManager _configManager;
		private readonly IList<AreaViewModel> _areas = new List<AreaViewModel>();
		private readonly IReadOnlyList<AreaViewModel> _areasReadOnly;
		private readonly IDictionary<Type, MenuSection> _menuSection2ViewModel = new Dictionary<Type, MenuSection>
		{
			{ typeof(MapViewModel), MenuSection.Map },
			{ typeof(RoutesViewModel), MenuSection.Routes },
			{ typeof(RouteStopsViewModel), MenuSection.RouteStops },
			{ typeof(PreferencesViewModel), MenuSection.Preferences },
			{ typeof(AboutViewModel), MenuSection.About },
			{ typeof(LicensesViewModel), MenuSection.Licenses }
		};

		private MenuSection _selectedMenuSection;
		private string _title = AppResources.map_view_title;

		public HomeViewModel(
					IMvxMessenger messenger, 
					IBusTrackerLocationService busTrackerLocationService,
					IConfigManager configManager)
		{
			this.MenuItems = new ReadOnlyObservableCollection<MenuViewModel>(this.CreateMenuViewModels());
			this.SelectMenuItemCommand = new MvxCommand<MenuSection>(this.SelectMenuItem);

			_messenger = messenger;
			_configManager = configManager;
			_locationService = busTrackerLocationService;
			_locationService.AreaChanged += (s, a) => this.UpdateTitle(a.Area);

			_taskChangedMessagesSubscription = _messenger.Subscribe<BackgroundTaskStateChangedMessage>(this.OnBackgroundTaskStateChanged);

			_areasReadOnly = new ReadOnlyCollection<AreaViewModel>(_areas);

			this.UpdateVehicleLocationsCommand = new MvxCommand(this.UpdateVehicleLocations);
		}

		public MvxCommand UpdateVehicleLocationsCommand { get; private set; }

		public ReadOnlyObservableCollection<MenuViewModel> MenuItems { get; private set; }

		public IReadOnlyList<AreaViewModel> Areas { get { return _areasReadOnly; } }

		public MvxCommand<MenuSection> SelectMenuItemCommand { get; private set; }

		public string Title 
		{
			get { return _title; }
			private set { this.RaiseAndSetIfChanged(ref _title, value, () => this.Title); }
		}

		public MenuSection GetSectionForViewModelType(Type type)
		{
			if (_menuSection2ViewModel.ContainsKey(type))
			{
				return _menuSection2ViewModel[type];
			}

			return MenuSection.Unknown;
		}

		public override void Start()
		{
			base.Start();

			this.FillAreas();
			this.UpdateTitle(_locationService.CurrentArea);
		}

		private void FillAreas()
		{
			var areaVMs = _configManager.GetConfig()
									    .Areas
									    .Select(a => new AreaViewModel(a.Id, this[string.Format("city_{0}_name", a.Id)]))
									    .ToList();

			foreach (var vm in areaVMs)
			{
				_areas.Add(vm);
			}
		}

		private ObservableCollection<MenuViewModel> CreateMenuViewModels()
		{
			return new ObservableCollection<MenuViewModel> 
			{
				new MenuViewModel 
				{ 
					Title = AppResources.map_view_title,
					Section = MenuSection.Map
				},
				new MenuViewModel 
				{
					Title = AppResources.routes_view_title,
					Section = MenuSection.Routes
				},
				new MenuViewModel 
				{
					Title = AppResources.route_stops_view_title,
					Section = MenuSection.RouteStops
				},
				new MenuViewModel 
				{ 
					Title = AppResources.preferences_view_title,
					Section = MenuSection.Preferences
				},
				new MenuViewModel
				{
					Title = AppResources.licenses_view_title,
					Section = MenuSection.Licenses
				},
				new MenuViewModel 
				{ 
					Title = AppResources.about_view_title,
					Section = MenuSection.About 
				}
			};
		}

		private void SelectMenuItem(MenuSection menuSection)
		{
			switch (menuSection)
			{
				case MenuSection.Map:
					this.ShowViewModel<MapViewModel>();
					_selectedMenuSection = MenuSection.Map;
					break;

				case MenuSection.Routes:
					this.ShowViewModel<RoutesViewModel>();
					_selectedMenuSection = MenuSection.Routes;
					this.IsBusy = false;
					break;

				case MenuSection.RouteStops:
					this.ShowViewModel<RouteStopsViewModel>();
					_selectedMenuSection = MenuSection.RouteStops;
					this.IsBusy = false;
					break;

				case MenuSection.Preferences:
					this.ShowViewModel<PreferencesViewModel>();
					_selectedMenuSection = MenuSection.Preferences;
					this.IsBusy = false;
					break;

				case MenuSection.Licenses:
					this.ShowViewModel<LicensesViewModel>();
					_selectedMenuSection = MenuSection.Licenses;
					this.IsBusy = false;
					break;

				case MenuSection.About:
					this.ShowViewModel<AboutViewModel>();
					_selectedMenuSection = MenuSection.About;
					this.IsBusy = false;
					break;
			}
		}

		private void UpdateTitle(Area area)
		{
			this.Title = area == null ? AppResources.map_view_title : area.Name;
		}

		private void OnBackgroundTaskStateChanged(BackgroundTaskStateChangedMessage message)
		{
			if (_selectedMenuSection == MenuSection.Map)
			{
				this.IsBusy = message.TaskState == BackgroundTaskState.Running;
			}
		}

		private void UpdateVehicleLocations()
		{
			_messenger.Publish(new VehicleLocationsUpdateRequestMessage(this));
		}
    }
}