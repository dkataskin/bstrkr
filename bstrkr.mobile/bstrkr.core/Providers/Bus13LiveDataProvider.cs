using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using RestSharp.Portable;

namespace bstrkr.core.providers
{
	public class Bus13LiveDataProvider : ILiveDataProvider
	{
		private string _endpoint;

		public Bus13LiveDataProvider(string endpoint)
		{
			_endpoint = endpoint;
		}

		public IEnumerable<Route> GetRoutes()
		{
			return null;
		}

		public async Task<IEnumerable<Route>> GetRoutesAsync()
		{
			var httpClient = new HttpClient();

			//var routes = await httpClient.GetStringAsync("http://bus13.ru/php/searchAllRouteTypes.php?city=saransk").ConfigureAwait(false);
			//return this.ParseRoutes(routes);

			var restClient = new RestClient();
			restClient.BaseUrl = new Uri(_endpoint);
			var request = new RestRequest("searchAllRouteTypes.php");
			request.AddParameter("city", "saransk", ParameterType.QueryString);

			var task = Task.Factory.StartNew(() =>
			{
				var response = restClient.Execute<string>(request).Result;
				return this.ParseRoutes(response.Data);
			}).ConfigureAwait(false);

			return task.GetAwaiter().GetResult();
		}

		public IEnumerable<Vehicle> GetVehicles()
		{
			return null;
		}

		public async Task<IEnumerable<Vehicle>> GetVehiclesAsync()
		{
			throw new NotImplementedException ();
		}

		private IEnumerable<Route> ParseRoutes(string routes)
		{
			if (string.IsNullOrEmpty(routes))
			{
				return new List<Route>();
			}

			return new List<Route>();
		}
	}
}