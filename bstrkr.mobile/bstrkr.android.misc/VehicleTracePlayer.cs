using System;

using Android.Gms.Maps.Model;

using System.Threading.Tasks;
using System.Linq;
using bstrkr.core.android.views;

namespace bstrkr.android.misc
{
	public class VehicleTracePlayer
	{
		private readonly Marker _marker;
		private readonly MapMarkerAnimationRunner _animator;
		private Task _task;

		public VehicleTracePlayer(Marker marker, MapMarkerAnimationRunner animator)
		{
			_marker = marker;
			_animator = animator;
		}

		public void Play(VehicleTrace vehicleTrace, IVehicleLocationEvaluator locationEvaluator)
		{
			_task = Task.Factory.StartNew(() => this.PlayTask(vehicleTrace, locationEvaluator));
		}

		private void PlayTask(VehicleTrace vehicleTrace, IVehicleLocationEvaluator locationEvaluator)
		{
			for (int i = 0; i < vehicleTrace.Updates.Count; i++)
			{
				var segments = locationEvaluator.Evaluate(vehicleTrace.Updates[i]);
				_animator.QueueAnimation(segments);

				if (i < vehicleTrace.Updates.Count - 1)
				{
					var timeSpan = vehicleTrace.Updates[i + 1].ReceivedAt - vehicleTrace.Updates[i].ReceivedAt;
					Task.Delay(timeSpan.TotalMilliseconds).Wait();
				}
			}
		}
	}
}