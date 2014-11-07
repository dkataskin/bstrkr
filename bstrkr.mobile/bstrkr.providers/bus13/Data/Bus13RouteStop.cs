using System;

namespace bstrkr.providers.bus13.data
{
	public class Bus13RouteStop
	{
		public int Id { get; set; }

		public string Name { get; set; }

		public string Descr { get; set; }

		public int Lat { get; set; }

		public int Lng { get; set; }

		public string Type { get; set; }
	}
}