using System;

using bstrkr.core;
using bstrkr.providers.bus13.data;

namespace bstrkr.grabber
{
	public interface IVehicleTraceOutputWriter
	{
		void Write(Bus13VehicleLocationUpdate update);
	}
}