using System;

using bstrkr.core.config;

namespace bstrkr.tests.services
{
	public class ConfigManagerStub : ConfigManagerBase
	{
		private string _config;

		public ConfigManagerStub(string config)
		{
			_config = config;
		}

		protected override string ReadConfigFile()
		{
			return _config;
		}
	}
}

