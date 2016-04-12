using System;

namespace bstrkr.core.services.location
{
    public interface IAreaPositioningService
    {
        event EventHandler<EventArgs> AreaLocated;

        event EventHandler<EventArgs> AreaLocatingFailed;

        Area Area { get; }

        bool DetectedArea { get; }

        void Start();

        void Stop();

        void SelectArea(Area area);
    }
}