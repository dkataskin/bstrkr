using System;

namespace bstrkr.core.config
{
	public class BusTrackerLocation
	{
		public string Name { get; set; }

		public string LocationId { get; set; }

		public double Latitude { get; set; }

		public double Longitude { get; set; }

		public string Endpoint { get; set; }

		public override bool Equals(object obj)
		{
			var otherLocation = obj as BusTrackerLocation;
			if (otherLocation == null)
			{
				return false;
			}

			if (string.IsNullOrEmpty(this.LocationId) ||
				string.IsNullOrEmpty(otherLocation.LocationId))
			{
				return false;
			}

			return this.LocationId.Equals(otherLocation.LocationId);
		}
	}
}