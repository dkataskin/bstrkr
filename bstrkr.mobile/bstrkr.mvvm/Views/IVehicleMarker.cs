using System;

using bstrkr.mvvm.viewmodels;

namespace bstrkr.mvvm.views
{
	public interface IVehicleMarker : IMarker
	{
		VehicleViewModel ViewModel { get; }
	}
}