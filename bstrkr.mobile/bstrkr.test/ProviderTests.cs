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
		[Test]
		public void CanGetRoutes()
		{
			var provider = new Bus13RouteDataService("http://bus13.ru/php/", "saransk");
			var task = provider.GetRoutesAsync().ConfigureAwait(false);
			var result = task.GetAwaiter().GetResult();

			Assert.IsNotNull(result);
		}

		[Test]
		public void Fail()
		{
			Assert.False (true);
		}

		[Test]
		[Ignore ("another time")]
		public void Ignore()
		{
			Assert.True (false);
		}

		[Test]
		public void Inconclusive()
		{
			Assert.Inconclusive ("Inconclusive");
		}
	}
}

