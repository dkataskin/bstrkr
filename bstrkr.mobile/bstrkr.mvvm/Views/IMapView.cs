using System;

namespace bstrkr.mvvm.views
{
	public interface IMapView
	{
		object MapObject { get; }

		double Zoom { get; }

		void SetCamera(double latitude, double longitude, double zoom);
	}
}