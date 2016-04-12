using System;

using Newtonsoft.Json;

namespace bstrkr.core.config
{
    public abstract class ConfigManagerBase : IConfigManager
    {
        private readonly Lazy<BusTrackerConfig> _config;

        public ConfigManagerBase()
        {
            _config = new Lazy<BusTrackerConfig>(this.ReadConfig);
        }

        public BusTrackerConfig GetConfig()
        {
            return _config.Value;
        }

        private BusTrackerConfig ReadConfig()
        {
            var configSource = this.ReadConfigFile();
            return JsonConvert.DeserializeObject<BusTrackerConfig>(configSource);
        }

        protected abstract string ReadConfigFile();
    }
}