using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

using Cirrious.MvvmCross.ViewModels;

namespace bstrkr.mvvm.viewmodels
{
	public class HomeViewModel : BusTrackerViewModelBase
    {
		private readonly IDictionary<Type, MenuSection> _menuSection2ViewModel = new Dictionary<Type, MenuSection>
		{
			{ typeof(MapViewModel), MenuSection.Map },
			{ typeof(RoutesViewModel), MenuSection.Routes },
			{ typeof(PreferencesViewModel), MenuSection.Preferences },
			{ typeof(AboutViewModel), MenuSection.About },
			{ typeof(LicensesViewModel), MenuSection.Licenses }
		};

		public HomeViewModel()
		{
			this.MenuItems = new ReadOnlyObservableCollection<MenuViewModel>(this.CreateMenuViewModels());
			this.SelectMenuItemCommand = new MvxCommand<MenuViewModel>(this.SelectMenuItem);
		}

		public ReadOnlyObservableCollection<MenuViewModel> MenuItems { get; private set; }

		public ICommand SelectMenuItemCommand { get; private set; }

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
					Title = "Map",
					Section = MenuSection.Map
				},
				new MenuViewModel 
				{
					Title = "Routes",
					Section = MenuSection.Routes
				},
				new MenuViewModel 
				{ 
					Title = "Preferences",
					Section = MenuSection.Preferences
				},
				new MenuViewModel
				{
					Title = "Licenses",
					Section = MenuSection.Licenses
				},
				new MenuViewModel 
				{ 
					Title = "About",
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