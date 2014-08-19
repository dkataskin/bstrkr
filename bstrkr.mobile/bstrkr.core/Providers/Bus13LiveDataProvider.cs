using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

using Newtonsoft.Json.Linq;

using RestSharp.Portable;
using RestSharp.Portable.Deserializers;

namespace bstrkr.core.providers
{
	public class Bus13LiveDataProvider : ILiveDataProvider
	{
		private const string RouteSplitter = "@ROUTE=";
		private const string RoutesResource = "searchAllRoutes.php";
		private const string RouteTypesResource = "searchAllRouteTypes.php";
		private const string LocationParam = "city";

		private string _endpoint;
		private string _location;

		public Bus13LiveDataProvider(string endpoint, string location)
		{
			_endpoint = endpoint;
			_location = location;
		}

		public IEnumerable<Route> GetRoutes()
		{
			return null;
		}

		public async Task<IEnumerable<Route>> GetRoutesAsync()
		{
			var client = this.GetRestClient();
			var routeTypes = await this.GetRouteTypesAsync(client).ConfigureAwait(false);

			var request = new RestRequest(RoutesResource);
			request.AddParameter(LocationParam, _location, ParameterType.QueryString);

			return await Task.Factory.StartNew(() =>
			{
				var response = client.Execute<JObject>(request).Result;

				return this.ParseRoutes(routeTypes, response.Data);
			}).ConfigureAwait(false);
		}

		public IEnumerable<Vehicle> GetVehicles()
		{
			return null;
		}

		public async Task<IEnumerable<Vehicle>> GetVehiclesAsync()
		{
			throw new NotImplementedException ();
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

			var routeList = new List<Route>();
			foreach (var routeType in routeTypes) 
			{
				var routeTypeRoutes = routesObject.Properties().FirstOrDefault(x => x.Name.Equals(routeType.typeName));
				if (routeTypeRoutes != null)
				{
					var routes = routeTypeRoutes.Value.ToString().Split(new[] { RouteSplitter }, StringSplitOptions.RemoveEmptyEntries);

					foreach (var route in routes)
					{
						routeList.Add(new Route 
						{
							Type = new RouteType(routeType.typeName, routeType.typeShName)
						});
					}
				}
			}

			return routeList;
		}

		private class Bus13RouteType
		{
			public string typeId { get; set; }

			public string typeName { get; set; }

			public string typeShName { get; set; }
		}
	}
}