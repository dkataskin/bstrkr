using System;

namespace bstrkr.core.android.views
{
    public class AnimationValueUpdatedEventArgs : EventArgs
    {
        public AnimationValueUpdatedEventArgs(object value)
        {
            this.Value = value;
        }

        public object Value { get; private set; }
    }
}