using System;
using System.Collections.ObjectModel;
using System.Linq;

using bstrkr.core;
using bstrkr.core.config;
using bstrkr.core.consts;
using bstrkr.core.services.location;
using bstrkr.mvvm.viewmodels;

using Cirrious.MvvmCross.ViewModels;

namespace bstrkr.mvvm.viewmodels
{
	public class SetAreaViewModel : BusTrackerViewModelBase
	{
		private readonly IAreaPositioningService _locationService;

		public SetAreaViewModel(IConfigManager configManager, IAreaPositioningService locationService)
		{
			_locationService = locationService;

			var areaVMs = configManager.GetConfig()
									   .Areas
									   .Select(a => new AreaViewModel(a, this[string.Format(AppConsts.AreaLocalizedNameStringKeyFormat, a.Id)]))
									   .ToList();
			
			this.Areas = new ObservableCollection<AreaViewModel>(areaVMs);

			this.SelectAreaCommand = new MvxCommand<int>(this.SelectAreaManually);
			this.CancelCommand = new MvxCommand(this.Cancel);
		}

		public MvxCommand<int> SelectAreaCommand { get; private set; }

		public MvxCommand CancelCommand { get; private set; }

		public ObservableCollection<AreaViewModel> Areas { get; private set; }

		private void SelectAreaManually(int index)
		{
			if (index >= 0)
			{
				_locationService.SelectArea(this.Areas[index].Area);
				this.ShowViewModel<HomeViewModel>();
			}
		}

		private void Cancel()
		{
			_locationService.SelectArea(null);
			this.ShowViewModel<HomeViewModel>();
		}
	}
}