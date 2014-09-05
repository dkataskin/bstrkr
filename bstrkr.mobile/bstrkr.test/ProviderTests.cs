using System;
using System.Threading.Tasks;

using NUnit.Framework;

using bstrkr.core.providers.bus13;
using bstrkr.core.providers;

namespace bstrkr.test
{
	[TestFixture]
	public class ProviderTests
	{
		private IBus13RouteDataService _service;

		[TestFixtureSetUp]
		public void SetUp()
		{
			_service = new Bus13RouteDataService("http://bus13.ru/php/", "saransk");
		}

		[Test]
		public void CanGetRoutes()
		{
			var task = _service.GetRoutesAsync().ConfigureAwait(false);
			var result = task.GetAwaiter().GetResult();

			Assert.IsNotNull(result);
			foreach (var route in result)
			{
				Console.WriteLine(route);

				Assert.IsNotNull(route.Id);
				Assert.IsNotNull(route.Name);
				Assert.IsNotNull(route.Type);
				Assert.IsNotNull(route.FirstStop);
				Assert.IsNotNull(route.LastStop);
			}
		}

		[Test]
		public void CanGetStops()
		{
			var task = _service.GetStopsAsync().ConfigureAwait(false);
			var result = task.GetAwaiter().GetResult();

			Assert.IsNotNull(result);
			foreach (var stop in result)
			{
				Console.WriteLine(stop);

				Assert.IsNotNull(stop.Id);
				Assert.IsNotNull(stop.Name);
			}
		}

		[Test]
		public void CanGetVehicles()
		{
			Assert.False (true);
		}
	}
}