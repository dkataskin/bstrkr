using System;

using Cirrious.MvvmCross.ViewModels;

namespace bstrkr.mvvm.viewmodels
{
	public class BusTrackerViewModelBase : MvxViewModel
	{
		private long _id;

		public long Id 
		{ 
			get { return _id; }
			protected set
			{
				if (_id != value)
				{
					_id = value;
					this.RaisePropertyChanged(() => this.Id);
				}
			}
		}
	}
}