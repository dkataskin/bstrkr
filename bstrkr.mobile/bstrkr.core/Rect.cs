using System;

namespace bstrkr.core
{
	public struct Rect
	{
		public Rect(Coords leftTop, Coords rightBottom)
		{
			this.LeftTop = leftTop;
			this.RightBottom = rightBottom;
		}

		public Coords LeftTop;

		public Coords RightBottom;

		public static Rect EarthWide
		{
			get { return new Rect(new Coords(0, 0), new Coords(90, 180)); }
		}
	}
}

