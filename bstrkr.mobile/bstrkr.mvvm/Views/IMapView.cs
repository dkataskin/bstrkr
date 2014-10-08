using System;

namespace bstrkr.mvvm.views
{
	public interface IMapView
	{
		void SetCamera(double latitude, double longitude, double zoom);
	}
}