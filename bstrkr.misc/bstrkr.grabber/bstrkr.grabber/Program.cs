using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using bstrkr.core.config;
using bstrkr.core.providers.bus13;
using bstrkr.core.spatial;

using CommandLine;

using RestSharp;
using RestSharp.Deserializers;

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
			var routes = service.GetRoutesAsync().Result;

			Console.WriteLine("Tracing {0}...", vehicleId);

			Task.Factory.StartNew(() =>
			{
				var timestamp = 0;
				while(true)
				{
					var response = service.GetVehicleLocationsAsync(routes, GeoRect.EarthWide, timestamp).Result;

					var vehicle = response.Updates.FirstOrDefault(x => x.Vehicle.Id.Equals(vehicleId));
					if (vehicle != null)
					{
						Console.WriteLine(
								"id:{0}, lat:{1}, lng:{2}",
								vehicleId, 
								vehicle.Vehicle.Location.Latitude,
								vehicle.Vehicle.Location.Longitude);
					}

					timestamp = response.Timestamp;
					Task.Delay(TimeSpan.FromSeconds(10)).Wait();
				}
			});
		}
	}
}