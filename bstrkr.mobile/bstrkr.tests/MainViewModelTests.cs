using System;

using NUnit.Framework;

using bstrkr.core.spatial;
using bstrkr.mvvm.viewmodels;
using bstrkr.tests.services;

namespace bstrkr.mobile.tests
{
	[TestFixture]
	public class MainViewModelTests
	{
		private const string Config = @"{
  ""locations"":[
    {
      ""name"": ""Саранск"",
	  ""locationId"": ""saransk"",
      ""latitude"": 54.1813447,
      ""longitude"": 45.1818003,
      ""endpoint"": ""http://bus13.ru/""
    },
    {
      ""name"": ""Рязань"",
	  ""locationId"": ""ryazan"",
      ""latitude"": 54.6137424,
      ""longitude"": 39.7211313,
      ""endpoint"": ""http://bus62.ru/""
    },
	{
      ""name"": ""Курск"",
	  ""locationId"": ""kursk"",
      ""latitude"": 51.7563926,
      ""longitude"": 36.1851959,
      ""endpoint"": ""http://bus46.ru/""
    }
	]
}";
		[SetUp]
		public void SetUp()
		{
		}

		[Test]
		public void CanDetectLocation()
		{
			var configManagerStub = new ConfigManagerStub("{}");
			var locationServiceStub = new LocationServiceStub(new GeoPoint(54.0, 45.0));


			var viewModel = new MainViewModel(configManagerStub, locationServiceStub);
		}
	}
}