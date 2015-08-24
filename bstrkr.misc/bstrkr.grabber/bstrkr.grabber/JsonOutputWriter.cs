using System;
using System.Collections.Generic;
using System.IO;

using bstrkr.core;
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
			try
			{
				var outputFile = Path.Combine(_outputDir, string.Format("trace-{0}.json", update.Vehicle.Id));
				Console.WriteLine ("writing {0}", outputFile);
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
							updateDTO.Waypoints.Add(
								new WaypointDTO 
								{
									Fraction = waypoint.Fraction,
									Latitude = waypoint.Location.Position.Latitude,
									Longitude = waypoint.Location.Position.Longitude,
									Heading = waypoint.Location.Heading
								});
						}
					}

					streamWriter.WriteLine(JsonConvert.SerializeObject(updateDTO, Formatting.Indented));
					streamWriter.Flush();
				}
			} 
			catch (Exception e)
			{
				Console.WriteLine("An error occured while writing json response: {0}", e);
			}
		}
	}
}