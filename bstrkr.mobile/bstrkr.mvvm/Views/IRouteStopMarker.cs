using System;

using bstrkr.mvvm.viewmodels;

namespace bstrkr.mvvm.views
{
	public interface IRouteStopMarker : IMapMarker
	{
		RouteStopMapViewModel ViewModel { get; }
	}
}