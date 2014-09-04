using System;
using System.Collections.Generic;

namespace bstrkr.core.providers.bus13
{
	public class VehicleLocationResponse
	{
		public int maxk { get; set; }

		public IList<Bus13VehicleLocation> anims { get; set; }
	}
}