using System.Collections.Generic;

using bstrkr.core;

using Cirrious.CrossCore.Platform;

using Newtonsoft.Json;
using System.Linq;

namespace bstrkr.mvvm.viewmodels
{
    public class LicensesViewModel : BusTrackerViewModelBase
    {
        public LicensesViewModel(IMvxResourceLoader resourceLoader)
        {
            var licensesInfo = JsonConvert.DeserializeObject<LicensesInfo>(resourceLoader.GetTextResource("licenses.json"));

            this.Licenses = licensesInfo.ThirdPartyStuff;
            this.Images = this.GetImageLicenseInfos(licensesInfo.Images);
        }

        public List<LicenseInfo> Licenses { get; private set; }

        public List<ImageInfo> Images { get; private set; }

        private List<ImageInfo> GetImageLicenseInfos(IEnumerable<ImageInfoLocalized> imageInfosLocalized)
        {
            return imageInfosLocalized.Select(x => new ImageInfo 
            { 
                City = this[x.CityId],
                Author = this[x.Author],
                Link = x.Link
            }).ToList();
        }
    }
}