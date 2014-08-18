using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

using RestSharp.Portable;
using RestSharp.Portable.Deserializers;
using Newtonsoft.Json.Linq;

namespace bstrkr.core.providers
{
	public class Bus13LiveDataProvider : ILiveDataProvider
	{
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

			var request = new RestRequest("searchAllRoutes.php");
			request.AddParameter("city", _location, ParameterType.QueryString);

			return await Task.Factory.StartNew(() =>
			{
				var response = client.Execute<JObject>(request).Result;
				//var routesObject = JObject.Parse(response.Data);

				foreach (var routeType in routeTypes) 
				{
				}

				return new List<Route>();
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
			var request = new RestRequest("searchAllRouteTypes.php");
			request.AddParameter("city", _location, ParameterType.QueryString);

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

		private IEnumerable<Route> ParseRoutes(string routes)
		{
			if (string.IsNullOrEmpty(routes))
			{
				return new List<Route>();
			}

			return new List<Route>();
		}

		private class Bus13RouteType
		{
			public string typeId { get; set; }

			public string typeName { get; set; }

			public string typeShName { get; set; }
		}
	}
}