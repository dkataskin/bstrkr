using System;

using bstrkr.core;
using bstrkr.core.spatial;

namespace bstrkr.core.services.location
{
	public interface IBusTrackerLocationService
	{
		event EventHandler<EventArgs> AreaChanged;

		Area Area { get; }

		bool DetectedArea { get; }

		void Start();

		void SelectArea(Area area);
	}
}