using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

using bstrkr.core;

using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;

namespace bstrkr.mvvm.viewmodels
{
	public class HomeViewModel : BusTrackerViewModelBase
    {
		private readonly IDictionary<Type, MenuSection> _menuSection2ViewModel = new Dictionary<Type, MenuSection>
		{
			{ typeof(MapViewModel), MenuSection.Map },
			{ typeof(RoutesViewModel), MenuSection.Routes },
			{ typeof(RouteStopsViewModel), MenuSection.RouteStops },
			{ typeof(PreferencesViewModel), MenuSection.Preferences },
			{ typeof(AboutViewModel), MenuSection.About },
			{ typeof(LicensesViewModel), MenuSection.Licenses }
		};

		public HomeViewModel()
		{
			this.MenuItems = new ReadOnlyObservableCollection<MenuViewModel>(this.CreateMenuViewModels());
			this.SelectMenuItemCommand = new MvxCommand<MenuViewModel>(this.SelectMenuItem);
			this.DetectLocationCommand = new MvxCommand(() => this.ShowViewModel<InitViewModel>());
		}

		public ReadOnlyObservableCollection<MenuViewModel> MenuItems { get; private set; }

		public MvxCommand<MenuViewModel> SelectMenuItemCommand { get; private set; }

		public MvxCommand DetectLocationCommand { get; private set; }

		public MenuSection GetSectionForViewModelType(Type type)
		{
			if (_menuSection2ViewModel.ContainsKey(type))
			{
				return _menuSection2ViewModel[type];
			}

			return MenuSection.Unknown;
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

		private void SelectMenuItem(MenuViewModel item)
		{
			switch (item.Section)
			{
				case MenuSection.Map:
					this.ShowViewModel<MapViewModel>(new { item.Id });
					break;

				case MenuSection.Routes:
					this.ShowViewModel<RoutesViewModel>(new { item.Id });
					break;

				case MenuSection.RouteStops:
					this.ShowViewModel<RouteStopsViewModel>(new { item.Id });
					break;

				case MenuSection.Preferences:
					this.ShowViewModel<PreferencesViewModel>(new { item.Id });
					break;

				case MenuSection.Licenses:
					this.ShowViewModel<LicensesViewModel>(new { item.Id });
					break;

				case MenuSection.About:
					this.ShowViewModel<AboutViewModel>(new { item.Id });
					break;
			}
		}
    }
}