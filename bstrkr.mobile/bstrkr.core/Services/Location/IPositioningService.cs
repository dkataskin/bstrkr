using System;

using bstrkr.core;
using bstrkr.core.spatial;

namespace bstrkr.core.services.location
{
	public interface IPositioningService
	{
		event EventHandler<AreaChangedEventArgs> AreaChanged;

		Area CurrentArea { get; }

		bool DetectedArea { get; }

		GeoPoint GetLastLocation();

		void Start();

		void Stop();
	}
}