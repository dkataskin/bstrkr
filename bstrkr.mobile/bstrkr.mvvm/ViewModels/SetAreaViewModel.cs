using System;

using System.Collections.ObjectModel;

using Cirrious.MvvmCross.ViewModels;

using bstrkr.core;
using bstrkr.core.config;
using bstrkr.core.services.location;
using bstrkr.mvvm.viewmodels;

namespace bstrkr.mvvm.viewmodels
{
	public class SetAreaViewModel : BusTrackerViewModelBase
	{
		private readonly IBusTrackerLocationService _locationService;

		public SetAreaViewModel(IConfigManager configManager, IBusTrackerLocationService locationService)
		{
			_locationService = locationService;

			var config = configManager.GetConfig();

			this.Areas = new ObservableCollection<Area>(config.Areas);

			this.SelectArea = new MvxCommand<int>(this.SelectAreaManually);
		}

		public MvxCommand<int> SelectArea { get; private set; }

		public ObservableCollection<Area> Areas { get; private set; }

		private void SelectAreaManually(int index)
		{
			_locationService.SelectArea(this.Areas[index]);
		}
	}
}