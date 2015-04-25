using System;
using System.IO;
using System.Reflection;

using bstrkr.core.config;

namespace bustracker.cli
{
	public class CliConfigManager : ConfigManagerBase
	{
		protected override string ReadConfigFile()
		{
			var location = new DirectoryInfo(Assembly.GetExecutingAssembly().Location);
			var directoryPath = Path.GetDirectoryName(location.FullName);
			return File.ReadAllText(Path.Combine(directoryPath, "config.json"));
		}
	}
}