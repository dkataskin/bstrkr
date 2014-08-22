using System;
using System.Threading.Tasks;

using NUnit.Framework;

using Providers;

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

				Assert.IsNotNull(route.Name);
				Assert.IsNotNull(route.Type);
				Assert.IsNotNull(route.FirstStop);
				Assert.IsNotNull(route.LastStop);
			}
		}

		[Test]
		public void CanGetVehicles()
		{
			Assert.False (true);
		}
	}
}

