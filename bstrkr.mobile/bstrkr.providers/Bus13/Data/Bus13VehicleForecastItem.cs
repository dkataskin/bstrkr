using System;

namespace bstrkr.providers.bus13.data
{
	public class Bus13VehicleForecastItem
	{
		public int Arrt { get; set; }

		public int StId { get; set; }

		public string StName { get; set; }

		public string StDescr { get; set; }

		public int Lat0 { get; set; }

		public int Lng0 { get; set; }

		public int Lat1 { get; set; }

		public int Lng1 { get; set; }
	}
}