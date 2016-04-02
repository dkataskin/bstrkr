using System;

namespace bstrkr.mvvm.viewmodels
{
	public class AreaViewModel : BusTrackerViewModelBase
	{
		public AreaViewModel(string areaId, string name)
		{
			this.AreaId = areaId;
			this.Name = name;
		}

		public string AreaId { get; private set; }

		public string Name { get; private set; }

		public override string ToString()
		{
			return this.Name;
		}
	}
}