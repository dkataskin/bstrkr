using System;

namespace bstrkr.core
{
	public struct Location
	{
		private static Location EmptyLocation = new Location(0, 0);

		public Location(float latitude, float longitude)
		{
			this.Latitude = latitude;
			this.Longitude = longitude;
		}

		public float Latitude;

		public float Longitude;

		public static Location Empty 
		{ 
			get { return EmptyLocation; }
		}

		public override string ToString()
		{
			return string.Format("({0}, {1})", this.Latitude, this.Longitude);
		}
	}
}

