using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace bstrkr.core
{
	public class Route
	{
		private IDictionary<string, object> _extendedProperties = new Dictionary<string, object>();

		public Route(
				string id, 
				string name, 
				RouteType type, 
				IEnumerable<RouteStop> stops, 
				IDictionary<string, object> extendedProperties)
		{
			this.Id = id;
			this.Name = name;
			this.Type = type;
			this.Stops = new ReadOnlyCollection<RouteStop>(stops.ToList());
			this.ExtendedProperties = new ReadOnlyDictionary<string, object>(_extendedProperties);

			if (extendedProperties != null)
			{
				foreach (var keyValuePair in extendedProperties)
				{
					_extendedProperties.Add(keyValuePair);
				}
			}
		}

		public string Id { get; private set; }

		public string Name { get; private set; }

		public RouteType Type { get; private set; }

		public IReadOnlyCollection<RouteStop> Stops { get; private set; }

		public RouteStop FirstStop { get; set; }

		public RouteStop LastStop { get; set; }

		public IReadOnlyDictionary<string, object> ExtendedProperties { get; private set; }

		public override string ToString()
		{
			return string.Format(
					"[Route: Id={0}, Name={1}, Type={2}, First Stop={3}, Last Stop={4}]", 
					this.Id,
					this.Name,
					this.Type,
					this.FirstStop,
					this.LastStop);
		}
	}
}