using System;
using System.Collections.Generic;

namespace bstrkr.providers.bus13.data
{
	public class Bus13VehicleLocation
	{
		public string id { get; set; }

		public int lon { get; set; }

		public int lat { get; set; }

		public int dir { get; set; }

		public string lasttime { get; set; }

		public string gos_num { get; set; }

		public int rid { get; set; }

		public string rnum { get; set; }

		public string rtype { get; set; }

		public int anim_key { get; set; }

		public string big_jumps { get; set; }

		public IList<Bus13AnimPoint> anim_points { get; set; }
	}
}

