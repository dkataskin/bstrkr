using System;
using System.Threading;

using Cirrious.MvvmCross.Test.Core;

using NUnit.Framework;

using bstrkr.core.config;
using bstrkr.core.services.location;
using bstrkr.core.spatial;
using bstrkr.mvvm.viewmodels;
using bstrkr.tests.infrastructure.services;

namespace bstrkr.mobile.tests
{
	[TestFixture]
	public class MainViewModelTests : ViewModelTestFixtureBase
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
		private IConfigManager _configManager;
		private ILocationService _locationService;

		private HomeViewModel _mainViewModel;

//		[SetUp]
//		public void SetUp()
//		{
//			_configManager = new ConfigManagerStub(Config);
//			_locationService = new LocationServiceStub(new GeoPoint(54.6, 39.7));
//
//			_mainViewModel = new HomeViewModel(_configManager, _locationService);
//		}

//		[Test]
//		public void CanDetectLocation()
//		{
//			_locationService.StartUpdating();
//
//			Thread.Sleep(900);
//
//			var location = _mainViewModel.;
//
//			Assert.IsNotNull(location);
//			Assert.AreEqual("ryazan", location.LocationId);
//		}
	}
}