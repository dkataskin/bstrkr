﻿using System.Collections.Generic;

using bstrkr.core;

using Cirrious.CrossCore.Platform;

using Newtonsoft.Json;

namespace bstrkr.mvvm.viewmodels
{
    public class LicensesViewModel : BusTrackerViewModelBase
    {
        public LicensesViewModel(IMvxResourceLoader resourceLoader)
        {
            this.Licenses = JsonConvert.DeserializeObject<LicensesInfo>(resourceLoader.GetTextResource("licenses.json"))
                                       .ThirdPartyStuff;
        }

        public List<LicenseInfo> Licenses { get; private set; }
    }
}