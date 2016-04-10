using System;

using bstrkr.core;

namespace bstrkr.mvvm.viewmodels
{
	public class AreaViewModel : BusTrackerViewModelBase
	{
		public AreaViewModel(Area area, string name)
		{
			this.Area = area;
			this.Name = name;
		}

		public Area Area { get; private set; }

		public string Name { get; private set; }

		public override string ToString()
		{
			return this.Name;
		}
	}
}