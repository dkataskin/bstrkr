﻿using System;

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
		private readonly IAreaPositioningService _locationService;

		public SetAreaViewModel(IConfigManager configManager, IAreaPositioningService locationService)
		{
			_locationService = locationService;

			var config = configManager.GetConfig();

			this.Areas = new ObservableCollection<Area>(config.Areas);

			this.SelectAreaCommand = new MvxCommand<int>(this.SelectAreaManually);
			this.CancelCommand = new MvxCommand(this.Cancel);
		}

		public MvxCommand<int> SelectAreaCommand { get; private set; }

		public MvxCommand CancelCommand { get; private set; }

		public ObservableCollection<Area> Areas { get; private set; }

		private void SelectAreaManually(int index)
		{
			if (index >= 0)
			{
				_locationService.SelectArea(this.Areas[index]);
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