using System;
using System.Collections.Generic;

namespace bstrkr.grabber
{
	public class VehicleAnim
	{
		public int anim_key { get; set; }
		public List<AnimPoint> anim_points { get; set; }
		public string big_jump { get; set; }
		public int dir { get; set; }
		public string gos_num { get; set; }
		public string id { get; set; }
		public string lasttime { get; set; }
		public int lat { get; set; }
		public int lon { get; set; }
		public int rid { get; set; }
		public string rnum { get; set; }
		public string rtype { get; set; }
	}
}