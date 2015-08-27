using System;
using bstrkr.core;

namespace bstrkr.android.misc
{
	public interface IVehicleLocationEvaluator
	{
		VehicleLocationUpdate Evaluate(VehicleLocationUpdateDTO vehicleLocationUpdate);
	}
}