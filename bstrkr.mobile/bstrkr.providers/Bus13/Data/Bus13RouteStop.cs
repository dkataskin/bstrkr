﻿using System;

namespace bstrkr.providers.bus13.data
{
	public class Bus13RouteStop
	{
		public int id { get; set; }

		public string name { get; set; }

		public string descr { get; set; }

		public int lat { get; set; }

		public int lng { get; set; }

		public string type { get; set; }
	}
}