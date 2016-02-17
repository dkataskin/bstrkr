using System;
using bstrkr.core.spatial;
using bstrkr.mvvm.maps;
using System.Timers;

namespace bstrkr.core.android.services.animation
{
	public class TimerAnimationRunner : IMarkerLocationAnimationRunner
	{
		private readonly Action _callback;
		private readonly Timer _timer;

		public TimerAnimationRunner(Action callback)
		{
//			IScheduler scheduler;
			_callback = callback;
		}

		public void RunAnimation(GeoPoint finalLocation, TimeSpan timespan)
		{
		}
	}
}