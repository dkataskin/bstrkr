using System;
using System.IO;

using Newtonsoft.Json;

namespace bstrkr.core.config
{
	public class ConfigManager : IConfigManager
	{
		private BusTrackerConfig _config;

		public ConfigManager(string config)
		{
			_config = this.ParseConfig(config);
		}

		public BusTrackerConfig GetConfig()
		{
			return _config;
		}

		private BusTrackerConfig ParseConfig(string config)
		{
			return JsonConvert.DeserializeObject<BusTrackerConfig>(config);
		}
	}
}