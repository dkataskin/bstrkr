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
			var restClient = new RestClient();
			restClient.BaseUrl = new Uri("http://google.com");
			var request = new RestRequest("resource");
			request.Method = HttpMethod.Get;
			//request.AddParameter("city", "saransk", ParameterType.QueryString);

			//var result = await restClient.Execute(request).ConfigureAwait(false);

			var client = new HttpClient();
			var result = await client.GetStringAsync(new Uri("http://google.com"));

			return null;
			//var response = restClient.Execute<string>(request).ConfigureAwait(false).GetAwaiter().GetResult();
			//return this.ParseRoutes(response.Data);
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