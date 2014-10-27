using System;
using System.Collections.Generic;

namespace bstrkr.providers.bus13.data
{
	public class Bus13VehicleLocation
	{
		public string Id { get; set; }

		public int Lon { get; set; }

		public int Lat { get; set; }

		public int Dir { get; set; }

		public string LastTime { get; set; }

		public string Gos_Num { get; set; }

		public int RId { get; set; }

		public string RNum { get; set; }

		public string RType { get; set; }

		public int Anim_Key { get; set; }

		public string Big_Jump { get; set; }

		public IList<Bus13AnimPoint> Anim_Points { get; set; }
	}
}

