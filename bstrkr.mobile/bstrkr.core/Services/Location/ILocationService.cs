using System;

using bstrkr.core.spatial;

namespace bstrkr.core.services.location
{
    public interface ILocationService
    {
        event EventHandler<LocationUpdatedEventArgs> LocationUpdated;

        event EventHandler<LocationErrorEventArgs> LocatingFailed;

        void StartUpdating();

        GeoPoint GetLastLocation();

        void StopUpdating();
    }
}