using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using bstrkr.core.config;
using bstrkr.core.providers.bus13;
using bstrkr.core.spatial;
using bstrkr.grabber;

using CommandLine;

using RestSharp;
using RestSharp.Deserializers;

using SharpKml.Base;
using SharpKml.Dom;
using SharpKml.Engine;

namespace bustracker.cli
{
	class MainClass
	{
		private static BusTrackerConfig Config;
		private static ConfiguredTaskAwaitable _traceTask;

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

					if (!string.IsNullOrEmpty(options.OutputDir) && !Directory.Exists(options.OutputDir))
					{
						Console.WriteLine("Directory {0} doesn't exist!", options.OutputDir);
						return;
					}

					IVehicleTraceOutputWriter outputWriter = null;
					if (string.IsNullOrEmpty(options.OutputType) || 
						string.Equals("kml", options.OutputType, StringComparison.InvariantCultureIgnoreCase))
					{
						outputWriter = new KmlOutputWriter(GetOutputDir(options));
					}

					if (string.Equals("json", options.OutputType, StringComparison.InvariantCultureIgnoreCase))
					{
						outputWriter = new JsonOutputWriter(GetOutputDir(options));
					}

					if (outputWriter == null)
					{
						outputWriter = new KmlOutputWriter(GetOutputDir(options));
					}

					Trace(service, options.VehicleId, outputWriter);
					Console.ReadKey();
				}
			}
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

		private static void Trace(IBus13RouteDataService service, string vehicleId, IVehicleTraceOutputWriter outputWriter)
		{
			Console.WriteLine("Retrieving routes...");
			var routes = service.GetRoutesAsync().Result;
			Console.WriteLine("{0} routes found...", routes.Count());

			Console.WriteLine("Tracing {0}...", vehicleId);
			_traceTask = Task.Factory.StartNew(() =>
			{
				var lastUpdate = DateTime.MinValue;
				var timestamp = 0;
				while (true)
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
							update.Vehicle.Location.Position.Latitude,
							update.Vehicle.Location.Position.Longitude,
							update.LastUpdate.ToString("u"),
							lastUpdate.ToString("u"));

						outputWriter.Write(update);
					}

					timestamp = response.Timestamp;

					ClearLine();
					Console.Write("waiting 10s...");
					Task.Delay(System.TimeSpan.FromSeconds(10)).Wait();
				}
			}).ConfigureAwait(false);
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

		private static string GetOutputDir(Options options)
		{
			return string.IsNullOrEmpty(options.OutputDir) ? 
						GetCurrentDirectory() :
						options.OutputDir;
		}
	}
}