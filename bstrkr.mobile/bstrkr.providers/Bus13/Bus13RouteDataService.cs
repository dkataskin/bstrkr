using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

using RestSharp.Portable;
using RestSharp.Portable.Deserializers;

using bstrkr.core;
using bstrkr.core.spatial;
using bstrkr.providers.bus13.data;

namespace bstrkr.core.providers.bus13
{
	public class Bus13RouteDataService : IBus13RouteDataService
	{
		private const string RoutesResource = "getRoutes.php";
		private const string StopsResource = "getStations.php";
		private const string ZonesResource = "getZones.php";
		private const string VehicleLocationResource = "getVehiclesMarkers.php";
		private const string RouteNodesResource = "getRouteNodes.php";
		private const string RouteIdFormatStr = "{0}-0";
		private const string LocationParam = "city";
		private const string TimestampParam = "curk";
		private const string RouteIdsParam = "rids";
		private const string RouteIdParam = "rid";
		private const string RouteTypeParam = "type";
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

			var bus13Routes = await this.ExecuteAsync<IList<Bus13Route>>(client, request).ConfigureAwait(false);

			return this.ParseRoutes(bus13Routes);
		}

		public async Task<GeoPolyline> GetRouteNodesAsync(Route route)
		{
			if (route == null)
			{
				throw new ArgumentException("Route must not be null", "route");
			}

			var request = this.GetRequestBase(RouteNodesResource);
			request.AddParameter(RouteIdParam, route.Id, ParameterType.QueryString);
			request.AddParameter(RouteTypeParam, 0, ParameterType.QueryString);

			var client = this.GetRestClient();
			var bus13GeoPoints = await this.ExecuteAsync<IEnumerable<Bus13GeoPoint>>(client, request).ConfigureAwait(false);

			return new GeoPolyline(bus13GeoPoints.Select(this.ParsePoint).ToList());
		}

		public async Task<VehicleLocationsResponse> GetVehicleLocationsAsync(
														IEnumerable<Route> routes,
														GeoRect rect,
														int timestamp)
		{
			if (routes == null || !routes.Any())
			{
				throw new ArgumentException("Routes collection must not be null or empty", "routes");
			}

			var request = this.GetRequestBase(VehicleLocationResource);
			request.AddParameter(
						RouteIdsParam, 
						string.Join(",", routes.Select(x => string.Format(RouteIdFormatStr, x.Id))),
						ParameterType.QueryString);

			request.AddParameter("lat0", this.CoordToInt(rect.LeftTop.Latitude), ParameterType.QueryString);
			request.AddParameter("lng0", this.CoordToInt(rect.LeftTop.Longitude), ParameterType.QueryString);
			request.AddParameter("lat1", this.CoordToInt(rect.RightBottom.Latitude), ParameterType.QueryString);
			request.AddParameter("lng1", this.CoordToInt(rect.RightBottom.Longitude), ParameterType.QueryString);

			request.AddParameter(TimestampParam, timestamp, ParameterType.QueryString);

			var client = this.GetRestClient();
			var response = await this.ExecuteAsync<Bus13VehicleLocationResponse>(client, request).ConfigureAwait(false);

			return new VehicleLocationsResponse(
										response.MaxK, 
										response.Anims.Select(x => this.ParseVehicleLocationUpdate(x)));
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

			var bus13Stops = await this.ExecuteAsync<IList<Bus13RouteStop>>(client, request).ConfigureAwait(false);

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

			return this.AddRandom(request);
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
					            bus13Route.Id, 
					            bus13Route.Name, 
								this.ParseRouteType(bus13Route.Type),
					            new List<RouteStop>(),
								new List<GeoPoint>());

				route.FirstStop = new RouteStop(
										bus13Route.FromStId.ToString(), 
										bus13Route.FromSt,
										string.Empty,
										GeoPoint.Empty);

				route.LastStop = new RouteStop(
										bus13Route.ToStId.ToString(),
										bus13Route.ToSt,
										string.Empty,
										GeoPoint.Empty);


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
										bus13RouteStop.Id.ToString(), 
						                bus13RouteStop.Name, 
						                bus13RouteStop.Descr,
						                this.ParsePoint(bus13RouteStop.Lat, bus13RouteStop.Lng));

				routeStops.Add(routeStop);
			}

			return routeStops;
		}

		private Bus13VehicleLocationUpdate ParseVehicleLocationUpdate(Bus13VehicleLocation bus13Vehicle)
		{
			var vehicle = new Vehicle 
			{
				Id = bus13Vehicle.Id,
				CarPlate = bus13Vehicle.Gos_Num,
				Location = this.ParsePoint(bus13Vehicle.Lat, bus13Vehicle.Lon),
				Heading = Convert.ToSingle(bus13Vehicle.Dir),
				RouteInfo = new VehicleRouteInfo
				{
					RouteId = bus13Vehicle.RId.ToString(),
					DisplayName = bus13Vehicle.RNum
				}
			};

			return new Bus13VehicleLocationUpdate
			{
				Vehicle = vehicle,
				LastUpdate = DateTime.Parse(bus13Vehicle.LastTime),
				Waypoints = bus13Vehicle.Anim_Points == null ? 
									new List<Waypoint>() :
									bus13Vehicle.Anim_Points.Select(x => new Waypoint(
																				this.ParsePoint(x.Lat, x.Lng),
																				Convert.ToSingle(int.Parse(x.Dir)),
																				int.Parse(x.Percent) / 100.0f))
															.ToList()
			};
		}

		private int CoordToInt(double coord)
		{
			return Convert.ToInt32(coord * 1000000);
		}

		private GeoPoint ParsePoint(Bus13GeoPoint bus13Point)
		{
			return this.ParsePoint(bus13Point.Lat, bus13Point.Lng);
		}

		private GeoPoint ParsePoint(string latitude, string longitude)
		{
			return this.ParsePoint(int.Parse(latitude), int.Parse(longitude));
		}

		private GeoPoint ParsePoint(int latitude, int longitude)
		{
			return new GeoPoint(latitude / 1000000f, longitude / 1000000f);
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

		private async Task<T> ExecuteAsync<T>(IRestClient client, IRestRequest request)
		{
			return await Task.Factory.StartNew(() => client.Execute<T>(request).Result.Data).ConfigureAwait(false);
		}
	}
}