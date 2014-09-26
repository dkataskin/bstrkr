using System;
using System.Linq;

using NUnit.Framework;

using bstrkr.core.config;

namespace bstrkr.mobile.tests
{
	[TestFixture]
	public class ConfigManagerTests
	{
		[Test]
		public void CanGetConfig()
		{
//			var configSource = @"{ locations: [ { 
//			""name"": ""test"",
//      		""latitude"": 54.0,
//      		""longitude"": 45.0,
//      		""endpoint"": ""http://bus13.ru/"" }]}";
//
//			var configManager = new ConfigManager(configSource);
//
//			var config = configManager.GetConfig();
//
//			Assert.IsNotNull(config);
//			Assert.IsNotNull(config.Locations);
//			Assert.IsTrue(config.Locations.Count == 1);
//
//			var location = config.Locations.First();
//			Assert.AreEqual(location.Name, "test");
//			Assert.AreEqual(location.Latitude, 54.0);
//			Assert.AreEqual(location.Longitude, 45.0);
//			Assert.AreEqual(location.Endpoint, "http://bus13.ru/");
		}
	}
}