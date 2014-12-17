using System;
using System.Collections.Generic;

using bstrkr.core;
using bstrkr.mvvm.viewmodels;

using Cirrious.CrossCore.Platform;

namespace bstrkr.mvvm.viewmodels
{
	public class LicensesViewModel : BusTrackerViewModelBase
	{
		public LicensesViewModel(IMvxResourceLoader resourceLoader)
		{
			this.Licenses = new List<LicenseInfo> 
			{
				new LicenseInfo("MVVMCross", "")
			};
		}

		public List<LicenseInfo> Licenses { get; private set; }
	}
}