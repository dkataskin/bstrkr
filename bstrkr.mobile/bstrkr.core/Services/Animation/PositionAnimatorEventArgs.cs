using System;

using bstrkr.core;

namespace bstrkr.core.services.animation
{
	public class PositionAnimatorEventArgs : EventArgs
	{
		public PositionAnimatorEventArgs(PathSegment segment)
		{
			this.PathSegment = segment;
		}

		public PathSegment PathSegment { get; private set; }
	}
}