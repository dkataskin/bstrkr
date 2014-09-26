using System;
using System.IO;

using Newtonsoft.Json;

using bstrkr.core.config;
using bstrkr.core.consts;

namespace bstrkr.core.ios.config
{
	public class ConfigManager : ConfigManagerBase
	{
		protected override string ReadConfigFile()
		{
			return File.ReadAllText(Path.Combine("./", AppConsts.ConfigFileName));
		}
	}
}