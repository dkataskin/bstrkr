using System.Collections.Generic;

using Android.Gms.Maps.Model;

namespace bstrkr.android.views
{
    public abstract class GoogleMapsSimpleMarkerBase : GoogleMapsMarkerBase
    {
        public Marker Marker { get; private set; }

        public abstract MarkerOptions GetSingleMarkerOptions();

        public override IDictionary<string, MarkerOptions> GetOptions()
        {
            return new Dictionary<string, MarkerOptions>
            {
                { "main", this.GetSingleMarkerOptions() }
            };
        }

        public override void AttachMarker(string key, Marker marker)
        {
            base.AttachMarker(key, marker);
            this.Marker = marker;
        }

        public override void DetachMarker(string key)
        {
            base.DetachMarker(key);
            this.Marker = null;
        }
    }
}