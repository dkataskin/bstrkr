using System;

using bstrkr.core.spatial;

namespace bstrkr.core.services.animation
{
    public class PositionAnimationValueChangedEventArgs : EventArgs
    {
        public PositionAnimationValueChangedEventArgs(GeoPoint newValue)
        {
            this.Value = newValue;
        }

        public GeoPoint Value { get; private set; }
    }
}