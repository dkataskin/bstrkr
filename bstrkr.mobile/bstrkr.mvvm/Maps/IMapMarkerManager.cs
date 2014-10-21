using System;
using System.Collections;

namespace bstrkr.mvvm.maps
{
	public interface IMapMarkerManager
	{
		IEnumerable ItemsSource { get; set; }
	}
}