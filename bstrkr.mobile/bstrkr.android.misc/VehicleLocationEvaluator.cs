using System;
using System.Collections.Generic;

using bstrkr.core;
using System.Linq;

namespace bstrkr.android.misc
{
	public class VehicleLocationEvaluator : IVehicleLocationEvaluator
	{
		public IEnumerable<PathSegment> Evaluate(VehicleLocationUpdateDTO update)
		{
			var animList = new List<PathSegment>();

			var totalTime = 15.0f;
			if (update.Waypoints != null && update.Waypoints.Any())
			{
				animList.Add(
					new PathSegment 
				{
					Duration = TimeSpan.FromSeconds(update.Waypoints[0].Fraction * totalTime),
					StartLocation = this.Location,
					FinalLocation = update.Waypoints[0].Location
				});

				for (int i = 0; i < update.Waypoints.Count - 1; i++)
				{
					animList.Add(
						new PathSegment 
					{
						Duration = TimeSpan.FromSeconds(update.Waypoints[i + 1].Fraction * totalTime),
						StartLocation = update.Waypoints[i].Location,
						FinalLocation = update.Waypoints[i + 1].Location
					});
				};
			}
			else
			{
				animList.Add(
					new PathSegment 
				{
					Duration = TimeSpan.FromSeconds(totalTime),
					StartLocation = this.Location,
					FinalLocation = update.Vehicle.Location
				});
			}
		}
	}
}