using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

using RestSharp.Portable;
using RestSharp.Portable.Deserializers;

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
			var routeTypes = await this.GetRouteTypesAsync().ConfigureAwait(false);

			var request = new RestRequest("searchAllRouteTypes.php");
			request.AddParameter("city", _location, ParameterType.QueryString);

			var client = this.GetRestClient();
			return await Task.Factory.StartNew(() =>
			{
				var response = client.Execute<string>(request).Result;
				return this.ParseRoutes(response.Data);
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

		private async Task<IEnumerable<RouteType>> GetRouteTypesAsync()
		{
			var request = new RestRequest("searchAllRouteTypes.php");
			request.AddParameter("city", _location, ParameterType.QueryString);

			var client = this.GetRestClient();
			return await Task.Factory.StartNew(() =>
			{
				try 
				{
					var response = client.Execute<RouteType[]>(request).Result;
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