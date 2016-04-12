using System;

using Android.Views;

namespace bstrkr.core.android.views
{
    public class ActionBarDrawerEventArgs : EventArgs
    {
        public View DrawerView { get; set; }

        public float SlideOffset { get; set; }

        public int NewState { get; set; }
    }
}