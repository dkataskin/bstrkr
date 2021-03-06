using System;
using System.Diagnostics;

using Cirrious.CrossCore.Platform;

namespace bstrkr.android
{
    public class DebugTrace : IMvxTrace
    {
        public void Trace(MvxTraceLevel level, string tag, Func<string> message)
        {
            Android.Util.Log.Info(tag, message());
            Debug.WriteLine(tag + ":" + level + ":" + message());
        }

        public void Trace(MvxTraceLevel level, string tag, string message)
        {
            Android.Util.Log.Info(tag, message);
            Debug.WriteLine(tag + ":" + level + ":" + message);
        }

        public void Trace(MvxTraceLevel level, string tag, string message, params object[] args)
        {
            try
            {
                Android.Util.Log.Info(tag, string.Format(message, args));
                Debug.WriteLine(string.Format(tag + ":" + level + ":" + message, args));
            }
            catch (FormatException)
            {
                Trace(MvxTraceLevel.Error, tag, "Exception during trace of {0} {1}", level, message);
            }
        }
    }
}
