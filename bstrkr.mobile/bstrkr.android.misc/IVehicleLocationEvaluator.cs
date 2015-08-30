using System;
using System.Collections.Generic;

using bstrkr.core;
using bstrkr.core.spatial;

namespace bstrkr.android.misc
{
	public interface IVehicleLocationEvaluator
	{
		IEnumerable<PathSegment> Evaluate(GeoLocation currentLocation, VehicleLocationUpdateDTO locationUpdate);
	}
}