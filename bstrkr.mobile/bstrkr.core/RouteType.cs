using System;

namespace bstrkr.core
{
	public class RouteType
	{
		public RouteType()
		{
		}

		public RouteType(string name, string shortName)
		{
			this.Name = name;
			this.ShortName = shortName;
		}

		public string Name { get; set; }

		public string ShortName { get; set; }
	}
}