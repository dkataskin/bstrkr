using System.Collections.Generic;
using System.Linq;

using bstrkr.core;
using bstrkr.core.consts;

using Cirrious.CrossCore.Platform;

using Newtonsoft.Json;

namespace bstrkr.mvvm.viewmodels
{
    public class ImageLicensesViewModel : BusTrackerViewModelBase
    {
        public ImageLicensesViewModel(IMvxResourceLoader resourceLoader)
        {
            var licensesInfo = JsonConvert.DeserializeObject<LicensesInfo>(resourceLoader.GetTextResource("licenses.json"));

            this.Images = this.GetImageLicenseInfos(licensesInfo.Images);
        }

        public List<ImageInfo> Images { get; private set; }

        private List<ImageInfo> GetImageLicenseInfos(IEnumerable<ImageInfoLocalized> imageInfosLocalized)
        {
            return imageInfosLocalized.Select(x => new ImageInfo 
            { 
                City = this[string.Format(AppConsts.AreaLocalizedNameStringKeyFormat, x.CityId)],
                Author = this[x.Author],
                Link = x.Link
            }).ToList();
        }
    }
}