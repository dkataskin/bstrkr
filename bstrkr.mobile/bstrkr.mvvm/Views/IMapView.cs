﻿using System;

using bstrkr.core.spatial;

namespace bstrkr.mvvm.views
{
	public interface IMapView
	{
		event EventHandler<EventArgs> ZoomChanged;

		object MapObject { get; }

		float Zoom { get; }

		void SetCamera(GeoPoint location, float zoom);

		void AddMarker(IMarker marker);

		void RemoveMarker(IMarker marker);
	}
}