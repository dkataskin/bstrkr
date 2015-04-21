using System;
using bstrkr.core.config;
using System.IO;

namespace bstrkr.grabber
{
	public class CliConfigManager : ConfigManagerBase
	{
		protected override string ReadConfigFile()
		{
			return File.ReadAllText("config.json");
		}
	}
}