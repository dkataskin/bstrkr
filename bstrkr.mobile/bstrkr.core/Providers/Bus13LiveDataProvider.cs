using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

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
			throw new NotImplementedException ();
		}

		public async Task<IEnumerable<Route>> GetRoutesAsync()
		{
			var httpClient = new HttpClient();
			var routes = await httpClient.GetStringAsync (this.GetRoutesRequestUri());
		}

		public IEnumerable<Vehicle> GetVehicles()
		{
			return null;
		}

		public async Task<IEnumerable<Vehicle>> GetVehiclesAsync()
		{
			throw new NotImplementedException ();
		}

		private string GetRoutesRequestUri()
		{
			return string.Format("{0}/", _endpoint);
		}

		private IEnumerable<Route> ParseRoutes(string routes)
		{
			if (string.IsNullOrEmpty(routes))
			{
				return new List<Route>();
			}

			var xmlDocument = new XDocument();
			xmlDocument.FromString()
		}
	}
}