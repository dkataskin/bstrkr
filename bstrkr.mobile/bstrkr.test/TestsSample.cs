using System;
using NUnit.Framework;
using bstrkr.core.providers;

namespace bstrkr.test
{
	[TestFixture]
	public class TestsSample
	{
		
		[SetUp]
		public void Setup()
		{
		}

		
		[TearDown]
		public void Tear()
		{
		}

		[Test]
		public void GetRoutes()
		{
			var provider = new Bus13LiveDataProvider("http://bus13.ru/php");
			var awaiter = provider.GetRoutesAsync().ConfigureAwait(false).GetAwaiter();
			var result = awaiter.GetResult();

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

