using System;

namespace bstrkr.core
{
	public class RouteType
	{
		public RouteType(string name, string shortName)
		{
			this.Name = name;
			this.ShortName = shortName;
		}

		public string Name { get; private set; }

		public string ShortName { get; private set; }

		public override string ToString()
		{
			return string.Format("[RouteType: ShortName={1}]", Name, ShortName);
		}
	}
}