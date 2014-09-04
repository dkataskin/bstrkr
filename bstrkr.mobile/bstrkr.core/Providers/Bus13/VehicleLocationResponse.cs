using System;
using System.Collections.Generic;

namespace Providers.Bus13
{
	public class VehicleLocationResponse
	{
		public int maxk { get; set; }

		public IList<Bus13VehicleLocation> anims { get; set; }
	}
}