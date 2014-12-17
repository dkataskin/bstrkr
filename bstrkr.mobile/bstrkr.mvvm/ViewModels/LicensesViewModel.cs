using System;
using System.Collections.Generic;

using bstrkr.core;
using bstrkr.mvvm.viewmodels;

using Cirrious.CrossCore.Platform;
using bstrkr.core.consts;

namespace bstrkr.mvvm.viewmodels
{
	public class LicensesViewModel : BusTrackerViewModelBase
	{
		public LicensesViewModel(IMvxResourceLoader resourceLoader)
		{
			this.Licenses = new List<LicenseInfo> 
			{
				new LicenseInfo(
						"MvvmCross", 
						resourceLoader.GetTextResource(string.Format("{0}/MVVMCross.txt", AppConsts.LicensesPath)))
			};
		}

		public List<LicenseInfo> Licenses { get; private set; }
	}
}