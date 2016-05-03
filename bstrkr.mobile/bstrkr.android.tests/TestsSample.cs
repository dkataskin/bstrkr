using System;
using NUnit.Framework;
using Android.Util;
using bstrkr.core.config;
using bstrkr.core.providers.bus13;
using System.Linq;
using bstrkr.core.spatial;

namespace bstrkr.android.tests
{
    [TestFixture]
    public class DataServiceTests
    {
        private BusTrackerConfig _config;

        [SetUp]
        public void Setup()
        {
            _config = MainActivity.ConfigManager.GetConfig();
        }

        [Test]
        public async void CanGetCheboksaryVehicleLocations()
        {
            var cheboksary = _config.Areas.FirstOrDefault(x => x.Id.Equals("cheboksary"));
            var service = new Bus13RouteDataService(cheboksary.Endpoint, cheboksary.DataServiceCode);
            var routes = await service.GetRoutesAsync();
            var vehicles = await service.GetVehicleLocationsAsync(routes, GeoRect.EarthWide, 0);

            Assert.IsTrue(vehicles.Updates.Count() > 0);
        }

        [Test]
        public async void CanGetRoutes()
        {
            foreach (var area in _config.Areas)
            {
                Console.WriteLine("Testing against {0}", area.Id);
                var service = new Bus13RouteDataService(area.Endpoint, area.DataServiceCode);
                var routes = await service.GetRoutesAsync();

                Assert.IsTrue(routes.Count() > 0);
            }
        }
    }
}