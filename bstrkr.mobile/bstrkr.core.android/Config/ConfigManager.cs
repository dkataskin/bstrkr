using Cirrious.CrossCore.Platform;

using bstrkr.core.config;
using bstrkr.core.consts;

namespace bstrkr.core.android.config
{
    public class ConfigManager : ConfigManagerBase
    {
        private readonly IMvxResourceLoader _resourceLoader;

        public ConfigManager(IMvxResourceLoader resourceLoader)
        {
            _resourceLoader = resourceLoader;
        }

        protected override string ReadConfigFile()
        {
            return _resourceLoader.GetTextResource(AppConsts.ConfigFileName);
        }
    }
}