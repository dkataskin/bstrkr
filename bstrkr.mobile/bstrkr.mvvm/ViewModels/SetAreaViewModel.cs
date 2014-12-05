using System;

using System.Collections.ObjectModel;

using bstrkr.core;
using bstrkr.core.config;
using bstrkr.mvvm.viewmodels;

namespace bstrkr.mvvm.viewmodels
{
	public class SetAreaViewModel : BusTrackerViewModelBase
	{
		public SetAreaViewModel(IConfigManager configManager)
		{
			var config = configManager.GetConfig();

			this.Areas = new ObservableCollection<Area>(config.Areas);
		}

		public ObservableCollection<Area> Areas { get; private set; }
	}
}