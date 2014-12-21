using System;
using System.Runtime.Serialization;

using bstrkr.core;

namespace bstrkr.providers.bus13.data
{
	[DataContract]
	public class Bus13Route
	{
		[DataMember(Name = "id")]
		public string Id { get; set; }

		[DataMember(Name = "name")]
		public string Name { get; set; }

		[DataMember(Name = "type")]
		public string Type { get; set; }

		[DataMember(Name = "num")]
		public string Num { get; set; }

		[DataMember(Name = "fromst")]
		public string FromSt { get; set; }

		[DataMember(Name = "fromstid")]
		public int FromStId { get; set; }

		[DataMember(Name = "tost")]
		public string ToSt { get; set; }

		[DataMember(Name = "tostid")]
		public int ToStId { get; set; }
	}
}