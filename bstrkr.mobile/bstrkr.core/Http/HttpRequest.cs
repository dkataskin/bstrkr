using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Http
{
	public class HttpRequest
	{
		private Dictionary<string, HttpParameter> _parameters = new Dictionary<string, HttpParameter>();

		public HttpRequest()
		{
			this.Parameters = new ReadOnlyDictionary<string, HttpParameter>(_parameters);
		}

		public string Path { get; set; }

		public ReadOnlyDictionary<string, HttpParameter> Parameters { get; private set; }

		public void AddParameter(string name, string value, HttpParameterType type = HttpParameterType.QueryString)
		{
			_parameters.Add(new HttpParameter(name, value, type));
		}

		private string RenderUri()
		{
		}
	}
}

