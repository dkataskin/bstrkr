using System;

using bstrkr.core;

namespace bstrkr.providers.bus13.data
{
	internal class Bus13Route
	{
		public string Id { get; set; }

		public string Name { get; set; }

		public string Type { get; set; }

		public string Num { get; set; }

		public string FromSt { get; set; }

		public int FromStId { get; set; }

		public string ToSt { get; set; }

		public int ToStId { get; set; }
	}
}