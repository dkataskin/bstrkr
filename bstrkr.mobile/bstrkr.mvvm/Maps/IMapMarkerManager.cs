using System;
using System.Collections;

using bstrkr.mvvm.views;

namespace bstrkr.mvvm.maps
{
	public interface IMapMarkerManager
	{
		IEnumerable ItemsSource { get; set; }

		T GetDataForMarker<T>(IMapMarker marker);
	}
}