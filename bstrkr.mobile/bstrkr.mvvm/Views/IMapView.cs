using System;

using bstrkr.core.spatial;

namespace bstrkr.mvvm.views
{
	public interface IMapView
	{
		event EventHandler<EventArgs> ZoomChanged;
		event EventHandler<EventArgs> CameraPositionChanged;

		object MapObject { get; }

		float Zoom { get; }

		GeoPoint CameraPosition { get; }

		void SetCamera(GeoPoint location, float zoom);

		void AddMarker(IMapMarker marker);

		void RemoveMarker(IMapMarker marker);
	}
}