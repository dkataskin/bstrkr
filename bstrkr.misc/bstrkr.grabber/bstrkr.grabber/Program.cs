using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using RestSharp;
using RestSharp.Deserializers;
using SharpKml.Engine;
using bstrkr.core.providers.bus13;
using bstrkr.core.spatial;
using System.Collections.Generic;

namespace bstrkr.grabber
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			if (args.Length < 2)
			{
				Console.WriteLine("Invalid input args!");
				return;
			}

			if (args[0] != "-e")
			{
				Console.WriteLine("Invalid input args!");
				return;
			}

			if (args.FirstOrDefault(x => string.Equals("-e", x)) == null)
			{
				Console.WriteLine("Target endpoint must be provided!");
				return;
			}

			var endpoint = args.First(x => string.Equals("-e", x));


			var service = new Bus13RouteDataService("http://bus13.ru/php/", "saransk");
			var routes = service.GetRoutesAsync().Result;
			var vehicles = service.GetVehicleLocationsAsync(routes, GeoRect.EarthWide, 0).Result;

			foreach (var vehicle in vehicles.Updates)
			{
				Console.WriteLine(vehicle);
			}

			Console.WriteLine("done");
			Console.ReadKey();

//			KmlFile kml = KmlFile.Create("test.kml", false);
		}
	}
}