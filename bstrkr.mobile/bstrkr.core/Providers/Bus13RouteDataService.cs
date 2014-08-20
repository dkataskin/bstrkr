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

		public Task<IEnumerable<Route>> GetRoutesAsync()
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<Vehicle>> GetVehicleLocationsAsync()
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<RouteStop>> GetRouteStopsAsync(Route route)
		{
			throw new NotImplementedException();
		}

//		public async Task<IEnumerable<Route>> GetRoutesAsync()
//		{
//			var client = this.GetRestClient();
//			var routeTypes = await this.GetRouteTypesAsync(client).ConfigureAwait(false);
//
//			var request = new RestRequest(RoutesResource);
//			request.AddParameter(LocationParam, _location, ParameterType.QueryString);
//
//			return await Task.Factory.StartNew(() =>
//			{
//				var response = client.Execute<JObject>(request).Result;
//
//				return this.ParseRoutes(routeTypes, response.Data);
//			}).ConfigureAwait(false);
//		}

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

			var routeList = new List<Route>();
			foreach (var routeType in routeTypes) 
			{
				var routeTypeRoutes = routesObject.Properties().FirstOrDefault(x => x.Name.Equals(routeType.typeName));
				if (routeTypeRoutes != null)
				{
					var routes = routeTypeRoutes.Value.ToString().Split(new[] { RoutesSplitter }, StringSplitOptions.RemoveEmptyEntries);

					foreach (var routeStr in routes)
					{
						var route = this.ParseRoute(routeStr, new RouteType(routeType.typeName, routeType.typeShName));
						routeList.Add(route);
					}
				}
			}

			return routeList;
		}

		private Route ParseRoute(string routeStr, RouteType routeType)
		{
			var routeParts = routeStr.Split(new[] { RouteSplitter }, StringSplitOptions.RemoveEmptyEntries);
			return new Route(
				routeParts[2], 
				routeParts[2], 
				routeType, 
				new List<RouteStop>());
		}

		private class Bus13RouteType
		{
			public string typeId { get; set; }

			public string typeName { get; set; }

			public string typeShName { get; set; }
		}
	}
}

