using System.Collections.Generic;

using System.Linq;

using bstrkr.core;

using Cirrious.CrossCore.Platform;

using Newtonsoft.Json;

namespace bstrkr.mvvm.viewmodels
{
    public class LicensesViewModel : BusTrackerViewModelBase
    {
        public LicensesViewModel(IMvxResourceLoader resourceLoader)
        {
            var licensesInfo = JsonConvert.DeserializeObject<LicensesInfo>(resourceLoader.GetTextResource("licenses.json"));

            this.Licenses = licensesInfo.ThirdPartyStuff;
        }

        public List<LicenseInfo> Licenses { get; private set; }
    }
}