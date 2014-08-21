using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

using RestSharp.Portable;
using RestSharp.Portable.Deserializers;

using bstrkr.core;

namespace Providers
{
	public class Bus13RouteDataService : IBus13RouteDataService
	{
		private const string RoutesSplitter = "@ROUTE=";
		private const string RouteSplitter = ";";
		private const string RoutesResource = "searchAllRoutes.php";
		private const string RouteTypesResource = "searchAllRouteTypes.php";
		private const string LocationParam = "city";

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
			var getRouteTypesTask = this.GetRouteTypesAsync(client);

			var request = new RestRequest(RoutesResource);
			request.AddParameter(LocationParam, _location, ParameterType.QueryString);

			var getRoutesTask = Task.Factory.StartNew(() =>
			{
				return client.Execute<JObject>(request).Result;
			});

			await Task.WhenAll(getRouteTypesTask, getRoutesTask).ConfigureAwait(false);

			return this.ParseRoutes(
						getRouteTypesTask.GetAwaiter().GetResult(), 
						getRoutesTask.GetAwaiter().GetResult().Data);
		}

		public Task<IEnumerable<Vehicle>> GetVehicleLocationsAsync()
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<RouteStop>> GetRouteStopsAsync(Route route)
		{
			throw new NotImplementedException();
		}

		private RestClient GetRestClient()
		{
			var client = new RestClient(_endpoint);
			client.ClearHandlers();

			client.RemoveHandler("text/html");
			client.AddHandler("text/html", new JsonDeserializer());

			return client;
		}

		private async Task<IEnumerable<Bus13RouteType>> GetRouteTypesAsync(IRestClient restClient)
		{
			var request = this.GetRequestBase(RouteTypesResource);

			return await Task.Factory.StartNew(() =>
			{
				try 
				{
					var response = restClient.Execute<Bus13RouteType[]>(request).Result;
					return response.Data;
				}
				catch (Exception ex)
				{
					return null;
				}
			}).ConfigureAwait(false);
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

		private IEnumerable<Route> ParseRoutes(IEnumerable<Bus13RouteType> routeTypes, JObject routesObject)
		{
			if (routeTypes == null | routesObject == null)
			{
				return null;
			}

			var routes = new List<Route>();
			foreach (var routeTypeData in routeTypes) 
			{
				var routeType = new RouteType(routeTypeData.typeName, routeTypeData.typeShName);
				var routeTypeRoutes = routesObject.Properties().FirstOrDefault(x => x.Name.Equals(routeType.Name));
				if (routeTypeRoutes != null)
				{
					var routesData = routeTypeRoutes.Value.ToString().Split(new[] { RoutesSplitter }, StringSplitOptions.RemoveEmptyEntries);
					foreach (var routeData in routesData)
					{
						routes.AddRange(this.ParseRouteData(routeData, routeType));
					}
				}
			}

			return routes;
		}

		private IEnumerable<Route> ParseRouteData(string routeStr, RouteType routeType)
		{
			var routes = new List<Route>();

			var routeParts = routeStr.Split(new[] { RouteSplitter }, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < routeParts.Length - 6; i++)
			{
				routes.Add(new Route(
								routeParts[i + 6], 
								routeParts[2], 
								routeType, 
								new List<RouteStop>()));
			}

			return routes;
		}

		private class Bus13RouteType
		{
			public string typeId { get; set; }

			public string typeName { get; set; }

			public string typeShName { get; set; }
		}
	}
}

