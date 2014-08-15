using System;

namespace Http
{
	public class HttpParameter
	{
		public HttpParameter()
		{
		}

		public HttpParameter(string name, string value) : this(name, value, HttpParameterType.QueryString)
		{
		}

		public HttpParameter(string name, string value, HttpParameterType type)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException("Name must not be null or empty.", "name");
			}

			if (string.IsNullOrWhiteSpace(value))
			{
				throw new ArgumentException("Value must not be null or empty.", "value");
			}

			this.Name = name;
			this.Value = value;
			this.Type = type;
		}

		public string Name { get; set; }

		public string Value { get; set; }

		public HttpParameterType Type { get; set; }
	}
}