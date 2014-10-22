using System;

namespace bstrkr.core.extensions
{
	public static class NumericExtensions
	{
		public static double ToRadians(this double val)
		{
			return (Math.PI / 180) * val;
		}

		public static double ToDegrees(this double val)
		{
			return val * (180.0 / Math.PI);
		}
	}
}