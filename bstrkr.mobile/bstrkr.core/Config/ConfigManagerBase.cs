using System;
using System.IO;

using Newtonsoft.Json;

namespace bstrkr.core.config
{
	public abstract class ConfigManagerBase : IConfigManager
	{
		private BusTrackerConfig _config;

		public ConfigManagerBase()
		{
			_config = this.ReadConfig();
		}

		public BusTrackerConfig GetConfig()
		{
			return _config;
		}

		private BusTrackerConfig ReadConfig()
		{
			var configSource = this.ReadConfigFile();
			return JsonConvert.DeserializeObject<BusTrackerConfig>(configSource);
		}

		protected abstract string ReadConfigFile();
	}
}