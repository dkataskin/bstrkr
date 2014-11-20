using System;

using bstrkr.core;
using bstrkr.core.spatial;

namespace bstrkr.core.services.location
{
	public interface IBusTrackerLocationService
	{
		event EventHandler<AreaChangedEventArgs> AreaChanged;

		Area Area { get; }

		GeoPoint Location { get; }

		double? Accuracy { get; }

		void Start();
	}
}