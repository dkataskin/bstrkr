using System;
using bstrkr.core;
using System.IO;
using System.Collections.Generic;
using bstrkr.providers.bus13.data;
using Newtonsoft.Json;

namespace bstrkr.grabber
{
	public class JsonOutputWriter : IVehicleTraceOutputWriter
	{
		private readonly string _outputDir;

		public JsonOutputWriter(string outputDir)
		{
			_outputDir = outputDir;
		}

		public void Write(Bus13VehicleLocationUpdate update)
		{
			var outputFile = Path.Combine(_outputDir, string.Format("trace-{0}.json", update.Vehicle.Id));
			using (var streamWriter = new StreamWriter(File.Open(outputFile, FileMode.Append)))
			{
				var updateDTO = new VehicleLocationUpdateDTO 
				{
					ReceivedAt = DateTime.UtcNow,
					VehicleId = update.Vehicle.Id,
					LastUpdatedAt = update.LastUpdate,
					Waypoints = new List<WaypointDTO>()
				};

				if (update.Waypoints != null)
				{
					foreach (var waypoint in update.Waypoints)
					{
						updateDTO.Waypoints.Add(new WaypointDTO 
						{
							Fraction = waypoint.Fraction,
							Location = waypoint.Location
						});
					}
				}

				streamWriter.WriteLine(JsonConvert.SerializeObject(updateDTO));
				streamWriter.Flush();
			}
		}
	}
}