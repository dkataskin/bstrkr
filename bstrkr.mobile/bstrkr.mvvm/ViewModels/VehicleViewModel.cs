using System;

using Cirrious.MvvmCross.ViewModels;

using bstrkr.core;

namespace bstrkr.mvvm.viewmodels
{
	public class VehicleViewModel : MvxViewModel
	{
		private Vehicle _vehicle;

		public VehicleViewModel(Vehicle vehicle)
		{
			_vehicle = vehicle;
		}

		public Vehicle Vehicle
		{
			get { return _vehicle; }
			private set { _vehicle = value; }
		}
	}
}