using System;

using Cirrious.MvvmCross.ViewModels;

namespace bstrkr.mvvm.viewmodels
{
	public class RouteViewModel : MvxViewModel
	{
		private string _name;

		public RouteViewModel()
		{
		}

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