using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

using RestSharp.Portable;
using RestSharp.Portable.Deserializers;

using bstrkr.core;

namespace bstrkr.core.providers.bus13
{
	public class Bus13RouteDataService : IBus13RouteDataService
	{
		private const string RoutesResource = "getRoutes.php";
		private const string StopsResource = "getStations.php";
		private const string ZonesResource = "getZones.php";
		private const string VehicleLocationResource = "getVehicleMarkers.php";
		private const string LocationParam = "city";
		private const string RandomParam = "_";

		private readonly Lazy<Random> _random = new Lazy<Random>();

		private string _endpoint;
		private string _location;

		public Bus13RouteDataService(string endpoint, string location)
		{
			if (string.IsNullOrWhiteSpace(endpoint))
			{
				throw new ArgumentException("Endpoint must not be null or empty.", "endpoint");
			}

			if (string.IsNullOrWhiteSpace(location))
			{
				throw new ArgumentException("Location must not be null or empty.", "location");
			}

			_endpoint = endpoint;
			_location = location;
		}

		public async Task<IEnumerable<Route>> GetRoutesAsync()
		{
			var client = this.GetRestClient();

			var request = this.GetRequestBase(RoutesResource);
			request = this.AddRandom(request);

			var bus13Routes = await Task.Factory.StartNew(() =>
			{
				return client.Execute<List<Bus13Route>>(request).Result.Data;
			}).ConfigureAwait(false);

			return this.ParseRoutes(bus13Routes);
		}

		public async Task<IEnumerable<Vehicle>> GetVehicleLocationsAsync()
		{
			throw new NotImplementedException();
		}

		public async Task<IEnumerable<RouteStop>> GetRouteStopsAsync(Route route)
		{
			throw new NotImplementedException();
		}

		public async Task<IEnumerable<RouteStop>> GetStopsAsync()
		{
			var client = this.GetRestClient();

			var request = this.GetRequestBase(StopsResource);
			request = this.AddRandom(request);

			var bus13Stops = await Task.Factory.StartNew(() =>
			{
				return client.Execute<List<Bus13RouteStop>>(request).Result.Data;
			});

			return this.ParseRouteStops(bus13Stops);
		}

		private RestClient GetRestClient()
		{
			var client = new RestClient(_endpoint);
			client.ClearHandlers();

			client.RemoveHandler("text/html");
			client.AddHandler("text/html", new JsonDeserializer());

			return client;
		}

		private IRestRequest GetRequestBase(string resource)
		{
			IRestRequest request = new RestRequest(resource);
			request = this.AddLocation(request, _location);

			return request;
		}

		private IRestRequest AddLocation(IRestRequest request, string location)
		{
			return request.AddParameter(LocationParam, location, ParameterType.QueryString);
		}

		// web client adds this random value to each and every request
		private IRestRequest AddRandom(IRestRequest request)
		{
			return request.AddParameter(RandomParam, _random.Value.NextDouble(), ParameterType.QueryString);
		}

		private IEnumerable<Route> ParseRoutes(IEnumerable<Bus13Route> bus13Routes)
		{
			if (bus13Routes == null)
			{
				return new List<Route>();
			}

			var routes = new List<Route>();
			foreach (var bus13Route in bus13Routes) 
			{
				var route = new Route(
					            bus13Route.id, 
					            bus13Route.name, 
								this.ParseRouteType(bus13Route.type),
					            new List<RouteStop>());

				route.FirstStop = new RouteStop(
										bus13Route.fromstid.ToString(), 
										bus13Route.fromst,
										string.Empty,
										Coords.Empty);

				route.LastStop = new RouteStop(
										bus13Route.tostid.ToString(),
										bus13Route.tost,
										string.Empty,
										Coords.Empty);


				routes.Add(route);
			}

			return routes;
		}

		private IEnumerable<RouteStop> ParseRouteStops(IEnumerable<Bus13RouteStop> bus13RouteStops)
		{
			if (bus13RouteStops == null)
			{
				return new List<RouteStop>();
			}

			var routeStops = new List<RouteStop>();
			foreach (var bus13RouteStop in bus13RouteStops)
			{
				var routeStop = new RouteStop(
										bus13RouteStop.id.ToString(), 
						                bus13RouteStop.name, 
						                bus13RouteStop.descr,
						                this.ParseLocation(bus13RouteStop.lat, bus13RouteStop.lng));

				routeStops.Add(routeStop);
			}

			return routeStops;
		}

		private Coords ParseLocation(int latitude, int longitude)
		{
			return new Coords(latitude / 1000000f, longitude / 1000000f);
		}

		private RouteType ParseRouteType(string routeType)
		{
			switch (routeType)
			{
				case "Т":
					return new RouteType("Троллейбс", routeType);

				case "М":
					return new RouteType("Маршрутное такси", routeType);

				case "А":
					return new RouteType("Автобус", routeType);

				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}