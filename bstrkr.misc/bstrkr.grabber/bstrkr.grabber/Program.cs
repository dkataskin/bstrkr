using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using bstrkr.core.config;
using bstrkr.core.providers.bus13;
using bstrkr.core.spatial;

using CommandLine;

using RestSharp;
using RestSharp.Deserializers;

using SharpKml.Base;
using SharpKml.Dom;
using SharpKml.Engine;

namespace bstrkr.grabber
{
	class MainClass
	{
		private static BusTrackerConfig Config;

		public static void Main(string[] args)
		{
			var options = new Options();
			var parser = new CommandLine.Parser();
			if (parser.ParseArguments(args, options))
			{
				var configManager = new CliConfigManager();
				Config = configManager.GetConfig();

				var configEntry = Config.Areas.FirstOrDefault(x => x.Id.Equals(options.ServiceEndpoint));
				if (configEntry == null)
				{
					Console.WriteLine("Invalid endpoint!");
					return;
				}

				var service = new Bus13RouteDataService(configEntry.Endpoint, configEntry.Id);
				if (options.List)
				{
					ListVehicles(service);
				}

				if (options.Trace)
				{
					if (string.IsNullOrEmpty(options.VehicleId))
					{
						Console.WriteLine("Vehicle id must be supplied!");
						return;
					}

					Trace(service, options.VehicleId);
					Console.ReadKey();
				}
			}


//			KmlFile kml = KmlFile.Create("test.kml", false);
		}

		private static void ListVehicles(IBus13RouteDataService service)
		{
			var routes = service.GetRoutesAsync().Result;
			var vehicles = service.GetVehicleLocationsAsync(routes, GeoRect.EarthWide, 0).Result;

			foreach (var update in vehicles.Updates)
			{
				Console.WriteLine(
						"id:{0} type:{1} route:{2}", 
						update.Vehicle.Id, 
						update.Vehicle.Type, 
						update.Vehicle.RouteInfo.RouteNumber);
			}
		}

		private static void Trace(IBus13RouteDataService service, string vehicleId)
		{
			var kmlWriter = GetKmlWriter(vehicleId);

			var routes = service.GetRoutesAsync().Result;

			Console.WriteLine("Tracing {0}...", vehicleId);
			Task.Factory.StartNew(() =>
			{
				var lastUpdate = DateTime.MinValue;
				var timestamp = 0;
				while(true)
				{
					ClearLine();
					Console.Write("retrieving vehicle locations...");
					var response = service.GetVehicleLocationsAsync(routes, GeoRect.EarthWide, timestamp).Result;

					var update = response.Updates.FirstOrDefault(x => x.Vehicle.Id.Equals(vehicleId));
					if (update != null)
					{
						lastUpdate = DateTime.Now;

						ClearLine();
						Console.WriteLine(
								"id:{0}, lat:{1}, lng:{2}, upd: {3}, rcvd:{4}",
								vehicleId, 
								update.Vehicle.Location.Latitude,
								update.Vehicle.Location.Longitude,
								update.LastUpdate.ToString("u"),
								lastUpdate.ToString("u"));

						kmlWriter.AddPoint(update.Vehicle.Location);
						
						if (update.Waypoints != null && update.Waypoints.Any())
						{
							var sortedWaypoints = update.Waypoints.OrderBy(x => x.Fraction).ToList();
							foreach (var waypoint in sortedWaypoints)
							{
								Console.WriteLine(
									"id:{0}, fr:{1:F2}, lat:{2}, lng:{3}",
									vehicleId, 
									waypoint.Fraction,
									waypoint.Location.Latitude,
									waypoint.Location.Longitude);

								kmlWriter.AddPoint(waypoint.Location);
							}
						}
					}

					kmlWriter.Save();

					timestamp = response.Timestamp;

					ClearLine();
					Console.Write("waiting 10s...");
					Task.Delay(System.TimeSpan.FromSeconds(10)).Wait();
				}
			});
		}

		private static void ClearLine()
		{
			Console.CursorLeft = 0;
			Console.Write(new String(' ', 50));
			Console.CursorLeft = 0;
		}

		private static string GetCurrentDirectory()
		{
			var location = new DirectoryInfo(Assembly.GetExecutingAssembly().Location);
			return Path.GetDirectoryName(location.FullName);
		}

		private static KmlOutputWriter GetKmlWriter(string vehicleId)
		{
			var document = new Document();
			document.Name = string.Format("Vehicle {0} trace", vehicleId);
			document.Description = new Description 
			{ 
				Text = string.Format(
					"Vehicle {0} trace, starts from {1}", 
					vehicleId,
					DateTime.Now.ToString("u"))
			};

			var style = new Style 
			{
				Id = "routePathStyle",
				Line = new LineStyle 
				{
					Color = Color32.Parse("7f00ffff"),
					Width = 4
				}
			};
			document.AddStyle(style);

			var placeMark = new Placemark 
			{
				Name = string.Format("Vehicle {0} path", vehicleId),
				StyleUrl = new Uri("#routePathStyle", UriKind.RelativeOrAbsolute)
			};

			var lineString = new LineString 
			{
				Extrude = true,
				AltitudeMode = AltitudeMode.ClampToGround,
				Coordinates = new CoordinateCollection(),
				Tessellate = true
			};

			placeMark.Geometry = lineString;
			document.AddFeature(placeMark);

			return new KmlOutputWriter 
			{
				KmlFile = KmlFile.Create(document, false),
				Path = lineString,
				OutputFile = Path.Combine(GetCurrentDirectory(), string.Format("trace-{0}.kml", vehicleId))
			};
		}

		private class KmlOutputWriter
		{
			public KmlFile KmlFile { get; set; }

			public LineString Path { get; set; }

			public string OutputFile { get; set; }

			public void AddPoint(GeoPoint point)
			{
				Path.Coordinates.Add(
					new Vector(
						point.Latitude,
						point.Longitude,
						1.0));
			}

			public void Save()
			{
				try 
				{
					using(var fileStream = File.OpenWrite(this.OutputFile))
					{
						KmlFile.Save(fileStream);
					}
				}
				catch (Exception e)
				{
					Console.WriteLine(
								"An error occured while saving {0}: {1}", 
								System.IO.Path.GetFileName(this.OutputFile), 
								e.ToString());
				}
			}
		}
	}
}