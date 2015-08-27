using System;
using System.Collections.Generic;

namespace bstrkr.grabber
{
	public class VehicleLocationUpdatesDTO
	{
		public VehicleLocationUpdatesDTO()
		{
			this.Updates = new List<VehicleLocationUpdateDTO>();
		}

		public List<VehicleLocationUpdateDTO> Updates { get; set; }
	}
}