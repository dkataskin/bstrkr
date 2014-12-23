using System;

namespace bstrkr.mvvm.viewmodels
{
	public class RouteStopViewModel : BusTrackerViewModelBase
	{
		private string _name;

		public string Name 
		{ 
			get
			{
				return _name;
			} 

			set
			{
				if (!string.Equals(_name, value))
				{
					_name = value;
					this.RaisePropertyChanged(() => this.Name);
				}
			} 
		}
	}
}