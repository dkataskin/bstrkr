using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using bstrkr.core;
using bstrkr.core.spatial;
using bstrkr.providers;
using bstrkr.providers.bus13.data;

using RestSharp.Portable;
using RestSharp.Portable.Deserializers;

using Xamarin;

namespace bstrkr.core.providers.bus13
{
	public class Bus13RouteDataService : IBus13RouteDataService
	{
		private const string RoutesResource = "getRoutes.php";
		private const string StopsResource = "getStations.php";
		private const string ZonesResource = "getZones.php";
		private const string VehicleLocationResource = "getVehiclesMarkers.php";
		private const string VehicleForecastResource = "getVehicleForecasts.php";
		private const string RouteStopForecastResource = "getStationForecasts.php";
		private const string RouteNodesResource = "getRouteNodes.php";
		private const string RouteIdFormatStr = "{0}-0";
		private const string LocationParam = "city";
		private const string TimestampParam = "curk";
		private const string RouteIdsParam = "rids";
		private const string RouteIdParam = "rid";
		private const string RouteTypeParam = "type";
		private const string RandomParam = "_";
		private const string InfoParam = "info";
		private const string InfoParamValue = "0123";

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

			var bus13Routes = await this.ExecuteAsync<List<Bus13Route>>(client, request).ConfigureAwait(false);

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
			var bus13GeoPoints = await this.ExecuteAsync<List<Bus13GeoPoint>>(client, request).ConfigureAwait(false);

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
						string.Join(
							",", 
							routes.Select(x => string.Join(",", x.Ids.Select(id => string.Format(RouteIdFormatStr, id))))),
						ParameterType.QueryString);

			request.AddParameter("lat0", this.CoordToInt(rect.NorthEast.Latitude), ParameterType.QueryString);
			request.AddParameter("lng0", this.CoordToInt(rect.NorthEast.Longitude), ParameterType.QueryString);
			request.AddParameter("lat1", this.CoordToInt(rect.SouthWest.Latitude), ParameterType.QueryString);
			request.AddParameter("lng1", this.CoordToInt(rect.SouthWest.Longitude), ParameterType.QueryString);

			request.AddParameter(TimestampParam, timestamp, ParameterType.QueryString);
			request.AddParameter(InfoParam, InfoParamValue, ParameterType.QueryString);

			var client = this.GetRestClient();
			var response = await this.ExecuteAsync<Bus13VehicleLocationResponse>(client, request)
									 .ConfigureAwait(false);

			var updates = new List<Bus13VehicleLocationUpdate>();

			if (response.Anims != null)
			{
				foreach (var rawUpdate in response.Anims)
				{
					try
					{
						updates.Add(this.ParseVehicleLocationUpdate(rawUpdate));
					} 
					catch (Exception e)
					{
						Insights.Report(e, Xamarin.Insights.Severity.Warning);
					}
				}
			}

			return new VehicleLocationsResponse(response.MaxK, updates);
		}

		public async Task<IEnumerable<RouteStop>> GetRouteStopsAsync(Route route)
		{
			throw new NotImplementedException();
		}

		public async Task<IEnumerable<RouteStop>> GetStopsAsync()
		{
			var client = this.GetRestClient();

			var request = this.GetRequestBase(StopsResource);

			var bus13Stops = await this.ExecuteAsync<List<Bus13RouteStop>>(client, request).ConfigureAwait(false);

			return this.ParseRouteStops(bus13Stops);
		}

		public async Task<VehicleForecast> GetVehicleForecastAsync(Vehicle vehicle)
		{
			var request = this.GetRequestBase(VehicleForecastResource);
			request.AddParameter("vid", vehicle.Id, ParameterType.QueryString);
			request.AddParameter("type", 0, ParameterType.QueryString);
			request.AddParameter(InfoParam, InfoParamValue, ParameterType.QueryString);

			var client = this.GetRestClient();
			var forecast = await this.ExecuteAsync<List<Bus13VehicleForecastItem>>(client, request).ConfigureAwait(false);

			if (forecast != null && forecast.Any())
			{
				return new VehicleForecast(vehicle, forecast.Select(this.ParseVehicleForecast).ToList());
			}

			return new VehicleForecast(vehicle, new List<VehicleForecastItem>());
		}

		public async Task<RouteStopForecast> GetRouteStopForecastAsync(string routeStopId)
		{
			var request = this.GetRequestBase(RouteStopForecastResource);
			request.AddParameter("sid", routeStopId, ParameterType.QueryString);
			request.AddParameter("type", 0, ParameterType.QueryString);
			request.AddParameter(InfoParam, InfoParamValue, ParameterType.QueryString);

			var client = this.GetRestClient();
			var forecast = await this.ExecuteAsync<List<Bus13RouteStopForecastItem>>(client, request)
									 .ConfigureAwait(false);

			if (forecast != null && forecast.Any())
			{
				return new RouteStopForecast(routeStopId, forecast.Select(this.ParseRouteStopForecast).ToList());
			}

			return new RouteStopForecast(routeStopId, new List<RouteStopForecastItem>());
		}

		private RestClient GetRestClient()
		{
			var client = new RestClient(_endpoint);
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
			var groupedRoutes = bus13Routes.GroupBy(x => new { Num = x.Num, Type = x.Type });
			foreach (var routeGroup in groupedRoutes) 
			{
				var routeSource = routeGroup.First();
				var route = new Route(
									routeSource.Id,
									routeGroup.Select(x => x.Id).ToList(),
									routeSource.Name,
									routeSource.Num,
						            new List<RouteStop>(),
									new List<GeoPoint>(),
									new List<VehicleTypes> { this.ParseVehicleType(routeSource.Type) });

				route.FirstStop = new RouteStop(
										routeSource.FromStId.ToString(), 
										routeSource.FromSt,
										string.Empty,
										GeoPoint.Empty);

				route.LastStop = new RouteStop(
										routeSource.ToStId.ToString(),
										routeSource.ToSt,
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
				Location = new GeoLocation(this.ParsePoint(bus13Vehicle.Lat, bus13Vehicle.Lon), Convert.ToSingle(bus13Vehicle.Dir)),
				Type = this.ParseVehicleType(bus13Vehicle.RType),
				RouteInfo = new VehicleRouteInfo
				{
					RouteId = bus13Vehicle.RId.ToString(),
					RouteNumber = bus13Vehicle.RNum,
					DisplayName = this.GetRouteDisplayName(bus13Vehicle.RNum, this.ParseVehicleType(bus13Vehicle.RType))
				}
			};

			var locationUpdate = new Bus13VehicleLocationUpdate
			{
				Vehicle = vehicle,
				LastUpdate = DateTime.ParseExact(bus13Vehicle.LastTime, "dd.MM.yyyy H:mm:ss", CultureInfo.InvariantCulture),
				Waypoints = new List<Waypoint>()
			};

			if (bus13Vehicle.Anim_Points != null && bus13Vehicle.Anim_Points.Any())
			{
				locationUpdate.Waypoints = bus13Vehicle.Anim_Points.Select(
																		x => new Waypoint(
																			this.ParsePoint(x.Lat, x.Lon),
																			Convert.ToSingle(int.Parse(x.Dir)),
																			int.Parse(x.Percent) / 100.0f))
																	.ToList();
			};

			return locationUpdate;
		}

		private VehicleForecastItem ParseVehicleForecast(Bus13VehicleForecastItem item)
		{
			var routeStop = new RouteStop(
									item.StId.ToString(), 
									item.StName, 
									item.StDescr, 
									this.ParsePoint(item.Lat0, item.Lng0));

			return new VehicleForecastItem(routeStop, item.Arrt);
		}

		private RouteStopForecastItem ParseRouteStopForecast(Bus13RouteStopForecastItem item)
		{
			var forecastItem = new RouteStopForecastItem();
			forecastItem.ArrivesInSeconds = item.Arrt;
			forecastItem.CurrentRouteStopName = item.Where;
			forecastItem.LastRouteStopName = item.LastSt;
			forecastItem.VehicleId = item.VehId;
			forecastItem.Route = new Route(
										item.RId.ToString(), 
										new[] { item.RId.ToString() },
										item.RNum,
										item.RNum,
										new List<RouteStop>(),
										new List<GeoPoint>(),
										new List<VehicleTypes> { this.ParseVehicleType(item.RType) });

			return forecastItem;
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

		private VehicleTypes ParseVehicleType(string vehicleType)
		{
			switch (vehicleType)
			{
				case "Т":
					return VehicleTypes.Trolley;
				
				case "Тр":
					return VehicleTypes.Tram;

				case "М":
					return VehicleTypes.MiniBus;

				case "А":
					return VehicleTypes.Bus;

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private async Task<T> ExecuteAsync<T>(IRestClient client, IRestRequest request)
		{
			var response = await client.Execute<T>(request);
			return response.Data;
		}

		private string GetRouteDisplayName(string routeNumber, VehicleTypes vehicleType)
		{
			switch (vehicleType)
			{
				case VehicleTypes.Bus:
					return string.Format(AppResources.bus_route_title_format, routeNumber);

				case VehicleTypes.MiniBus:
					return string.Format(AppResources.minibus_route_title_format, routeNumber);

				case VehicleTypes.Trolley:
					return string.Format(AppResources.troll_route_title_format, routeNumber);

				case VehicleTypes.Tram:
					return string.Format(AppResources.tramway_route_title_format, routeNumber);

				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}