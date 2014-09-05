using System;

using bstrkr.core;

namespace bstrkr.providers.bus13.data
{
	internal class Bus13Route
	{
		public string id { get; set; }

		public string name { get; set; }

		public string type { get; set; }

		public string num { get; set; }

		public string fromst { get; set; }

		public int fromstid { get; set; }

		public string tost { get; set; }

		public int tostid { get; set; }
	}
}