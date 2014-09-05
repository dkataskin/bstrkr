using System;
using System.Collections.Generic;

namespace bstrkr.providers.bus13.data
{
	public class Bus13VehicleLocationResponse
	{
		public int maxk { get; set; }

		public IList<Bus13VehicleLocation> anims { get; set; }
	}
}