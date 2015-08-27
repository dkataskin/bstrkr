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
		private readonly VehicleLocationUpdatesDTO _updates = new VehicleLocationUpdatesDTO();
		private readonly JsonSerializer _serializer = new JsonSerializer();

		public JsonOutputWriter(string outputDir)
		{
			_outputDir = outputDir;
			_serializer.Formatting = Formatting.Indented;
		}

		public void Write(Bus13VehicleLocationUpdate update)
		{
			try
			{
				var outputFile = Path.Combine(_outputDir, string.Format("trace-{0}.json", update.Vehicle.Id));
				using (var streamWriter = new StreamWriter(File.OpenWrite(outputFile)))
				using (var jsonTextWriter = new JsonTextWriter(streamWriter))
				{
					var updateDTO = new VehicleLocationUpdateDTO 
					{
						ReceivedAt = DateTime.UtcNow,
						VehicleId = update.Vehicle.Id,
						LastUpdatedAt = update.LastUpdate,
						Latitude = update.Vehicle.Location.Position.Latitude,
						Longitude = update.Vehicle.Location.Position.Longitude,
						Heading = update.Vehicle.Location.Heading,
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

					_updates.Updates.Add(updateDTO);

					_serializer.Serialize(jsonTextWriter, _updates);
					jsonTextWriter.Flush();
				}
			} 
			catch (Exception e)
			{
				Console.WriteLine("An error occured while writing json response: {0}", e);
			}
		}
	}
}