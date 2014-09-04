using System;

namespace bstrkr.core
{
	public struct Coords
	{
		private static Coords EmptyLocation = new Coords(0, 0);

		public Coords(float latitude, float longitude)
		{
			this.Latitude = latitude;
			this.Longitude = longitude;
		}

		public float Latitude;

		public float Longitude;

		public static Coords Empty 
		{ 
			get { return EmptyLocation; }
		}

		public override string ToString()
		{
			return string.Format("({0}, {1})", this.Latitude, this.Longitude);
		}
	}
}

