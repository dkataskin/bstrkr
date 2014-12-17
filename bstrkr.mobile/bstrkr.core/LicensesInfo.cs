using System;

namespace bstrkr.core
{
	public class LicenseInfo
	{
		public LicenseInfo()
		{
		}

		public LicenseInfo(string name, string license)
		{
			this.Name = name;
			this.License = license;
		}

		public string Name { get; set; }

		public string License { get; set; }
	}
}