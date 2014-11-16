using System;

using Cirrious.MvvmCross.ViewModels;

using bstrkr.core.config;

namespace bstrkr.mvvm.viewmodels
{
	public class SettingsViewModel : BusTrackerViewModelBase
	{
		public BusTrackerLocation Location { get; set; }
	}
}