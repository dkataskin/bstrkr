using System;

using NUnit.Framework;

using bstrkr.core.spatial;

namespace bstrkr.tests
{
	[TestFixture]
	public class SpatialTests
	{
		[Test]
		public void CanCalculateDistance()
		{
			var point1 = new GeoPoint(50.072091, -5.715071);
			var point2 = new GeoPoint(58.644034, -3.069973);

			var distance = point1.DistanceTo(point2);
			var alpha = 0.1;

			Assert.IsTrue(Math.Abs(distance - 968.2) < alpha);
		}
	}
}