﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using bstrkr.core.spatial;
using bstrkr.providers;
using bstrkr.providers.bus13.data;
using bstrkr.providers.postprocessors;

using RestSharp.Portable;
using RestSharp.Portable.Deserializers;
using RestSharp.Portable.HttpClient;

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
        private const string RouteIdFormatStr = "{0}-{1}";
        private const string LocationParam = "city";
        private const string TimestampParam = "curk";
        private const string RouteIdsParam = "rids";
        private const string RouteIdParam = "rid";
        private const string RouteTypeParam = "type";
        private const string RandomParam = "_";
        private const string InfoParam = "info";
        private const string InactiveItem = "_";

        private readonly Lazy<Random> _random = new Lazy<Random>();
        private readonly IEnumerable<IRouteStopsDataPostProcessor> _routeStopPostProcessors;

        private readonly string _endpoint;
        private readonly string _location;
        private readonly string _infoParamValue = "01234";

        public Bus13RouteDataService(string endpoint, string location, string infoParam)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                throw new ArgumentException("Endpoint must not be null or empty.", nameof(endpoint));
            }

            if (string.IsNullOrWhiteSpace(location))
            {
                throw new ArgumentException("Location must not be null or empty.", nameof(location));
            }

            if (string.IsNullOrWhiteSpace(infoParam))
            {
                throw new ArgumentException("Info param value must not be null or empty.", nameof(infoParam));
            }

            _infoParamValue = infoParam;
            _endpoint = endpoint;
            _location = location;

            var context = new DataServiceContext(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);
            var postProcessorsFactory = new PostProcessorFactory();

            _routeStopPostProcessors = postProcessorsFactory.CreateRouteStopsDataPostProcessors(context);
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
                throw new ArgumentException("Route must not be null", nameof(route));
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
                throw new ArgumentException("Routes collection must not be null or empty", nameof(routes));
            }

            var request = this.GetRequestBase(VehicleLocationResource);
            request.AddParameter(
                        RouteIdsParam,
                        string.Join(
                            ",",
                            routes.Select(this.GetRouteKey).ToList()),
                            ParameterType.QueryString);

            request.AddParameter("lat0", this.CoordToInt(rect.NorthEast.Latitude), ParameterType.QueryString);
            request.AddParameter("lng0", this.CoordToInt(rect.NorthEast.Longitude), ParameterType.QueryString);
            request.AddParameter("lat1", this.CoordToInt(rect.SouthWest.Latitude), ParameterType.QueryString);
            request.AddParameter("lng1", this.CoordToInt(rect.SouthWest.Longitude), ParameterType.QueryString);

            request.AddParameter(TimestampParam, timestamp, ParameterType.QueryString);
            request.AddParameter(InfoParam, _infoParamValue, ParameterType.QueryString);

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
                        Insights.Report(e);
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

            return this.ApplyPostProcessor(this.ParseRouteStops(bus13Stops));
        }

        public async Task<VehicleForecast> GetVehicleForecastAsync(Vehicle vehicle)
        {
            var request = this.GetRequestBase(VehicleForecastResource);
            request.AddParameter("vid", vehicle.Id, ParameterType.QueryString);
            request.AddParameter("type", 0, ParameterType.QueryString);
            request.AddParameter(InfoParam, _infoParamValue, ParameterType.QueryString);

            var client = this.GetRestClient();
            var forecast = await this.ExecuteAsync<List<Bus13VehicleForecastItem>>(client, request).ConfigureAwait(false);

            if (forecast != null && forecast.Any())
            {
                return new VehicleForecast(vehicle, forecast.Select(x => this.ParseVehicleForecast(x, vehicle.Type)).ToList());
            }

            return new VehicleForecast(vehicle, new List<VehicleForecastItem>());
        }

        public async Task<RouteStopForecast> GetRouteStopForecastAsync(RouteStop routeStop)
        {
            var bus13RouteStop = routeStop.VendorInfo as Bus13RouteStop;
            if (bus13RouteStop == null)
            {
                int id = 0;
                int.TryParse(routeStop.Id, out id);
                bus13RouteStop = new Bus13RouteStop { Id = id, Type = "0" };
            }

            var request = this.GetRequestBase(RouteStopForecastResource);
            request.AddParameter("sid", bus13RouteStop.Id, ParameterType.QueryString);
            request.AddParameter("type", bus13RouteStop.Type, ParameterType.QueryString);
            request.AddParameter(InfoParam, _infoParamValue, ParameterType.QueryString);

            var client = this.GetRestClient();
            var forecast = await this.ExecuteAsync<List<Bus13RouteStopForecastItem>>(client, request)
                                     .ConfigureAwait(false);

            if (forecast != null && forecast.Any())
            {
                return new RouteStopForecast(routeStop.Id, forecast.Select(this.ParseRouteStopForecast).ToList());
            }

            return new RouteStopForecast(routeStop.Id, new List<RouteStopForecastItem>());
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

        private string GetRouteKey(Route route)
        {
            var routeType = 0;
            if (route.VehicleType == VehicleTypes.Tram)
            {
                routeType = 1;
            }

            if (route.VendorInfo == null)
            {
                return string.Format(RouteIdFormatStr, route.Id, routeType);
            };

            return string.Format(RouteIdFormatStr, (route.VendorInfo as Bus13Route).Id, routeType);
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
                if (bus13Route.Num == InactiveItem || bus13Route.Type == InactiveItem ||
                    string.IsNullOrEmpty(bus13Route.Num) || string.IsNullOrEmpty(bus13Route.Type))
                {
                    continue;
                }

                var route = new Route(
                                    GenerateId(bus13Route),
                                    bus13Route.Name,
                                    bus13Route.Num,
                                    this.ParseVehicleType(bus13Route.Type),
                                    new List<RouteStop>(),
                                    new List<GeoPoint>());
                route.VendorInfo = bus13Route;

                route.FirstStop = this.ParseRouteStop(
                                            bus13Route.FromStId,
                                            bus13Route.FromSt,
                                            string.Empty,
                                            route.VehicleType,
                                            GeoPoint.Empty);

                route.LastStop = this.ParseRouteStop(
                                            bus13Route.ToStId,
                                            bus13Route.ToSt,
                                            string.Empty,
                                            route.VehicleType,
                                            GeoPoint.Empty);

                routes.Add(route);
            }

            return routes;
        }

        private string GenerateId(Bus13Route sourceRoute)
        {
            if (sourceRoute == null)
            {
                return string.Empty;
            }

            return $"{sourceRoute.Id}_{sourceRoute.Num}_{sourceRoute.Type}";
        }

        private string GenerateId(Bus13RouteStop sourceStop)
        {
            if (sourceStop == null)
            {
                return string.Empty;
            }

            return $"{sourceStop.Id}_{sourceStop.Type}";
        }

        private RouteStop ParseRouteStop(int id, string name, string description, VehicleTypes vehicleType, GeoPoint location)
        {
            var bus13RouteStop = new Bus13RouteStop
            {
                Id = id,
                Name = name,
                Descr = description,
                Type = this.ConvertVehicleTypeToTransportType(vehicleType).ToString(),
                Lat = Convert.ToInt32(location.Latitude * 1000000),
                Lng = Convert.ToInt32(location.Longitude * 1000000)
            };

            var routeStop = new RouteStop(this.GenerateId(bus13RouteStop), name, description, location)
            {
                VendorInfo = bus13RouteStop
            };

            return this.ApplyPostProcessor(routeStop);
        }

        private RouteStop ParseRouteStop(Bus13RouteStop sourceRouteStop)
        {
            var routeStop = new RouteStop(
                this.GenerateId(sourceRouteStop),
                sourceRouteStop.Name,
                sourceRouteStop.Descr,
                this.ParsePoint(sourceRouteStop.Lat, sourceRouteStop.Lng))
            {
                VendorInfo = sourceRouteStop
            };

            return this.ApplyPostProcessor(routeStop);
        }

        private int ConvertVehicleTypeToTransportType(VehicleTypes vehicleTypes)
        {
            if (vehicleTypes == VehicleTypes.Tram)
            {
                return 1;
            }

            return 0;
        }

        private IEnumerable<RouteStop> ParseRouteStops(IEnumerable<Bus13RouteStop> bus13RouteStops)
        {
            if (bus13RouteStops == null)
            {
                return new List<RouteStop>();
            }

            return bus13RouteStops.Select(this.ParseRouteStop).ToList();
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
                    RouteId = this.GenerateId(
                                    new Bus13Route
                                    {
                                        Id = bus13Vehicle.RId.ToString(),
                                        Num = bus13Vehicle.RNum,
                                        Type = bus13Vehicle.RType
                                    }),
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

        private VehicleForecastItem ParseVehicleForecast(Bus13VehicleForecastItem item, VehicleTypes vehicleType)
        {
            var routeStop = this.ParseRouteStop(item.StId, item.StName, item.StDescr, vehicleType, GeoPoint.Empty);

            return new VehicleForecastItem(routeStop, item.Arrt);
        }

        private RouteStopForecastItem ParseRouteStopForecast(Bus13RouteStopForecastItem item)
        {
            var bus13Route = new Bus13Route { Id = item.RId.ToString(), Num = item.RNum, Type = item.RType };
            var forecastItem = new RouteStopForecastItem
            {
                ArrivesInSeconds = item.Arrt,
                CurrentRouteStopName = item.Where,
                LastRouteStopName = item.LastSt,
                VehicleId = item.VehId,
                Route = new Route(
                    this.GenerateId(bus13Route),
                    item.RNum,
                    item.RNum,
                    this.ParseVehicleType(item.RType),
                    new List<RouteStop>(),
                    new List<GeoPoint>()) {VendorInfo = bus13Route}
            };

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

        private RouteStop ApplyPostProcessor(RouteStop routeStop)
        {
            return this.ApplyPostProcessor(new[] { routeStop }).Single();
        }

        private IEnumerable<RouteStop> ApplyPostProcessor(IEnumerable<RouteStop> routeStops)
        {
            return _routeStopPostProcessors.Aggregate(routeStops, (current, postProcessor) => postProcessor.Process(current))
                                           .ToList();
        }
    }
}