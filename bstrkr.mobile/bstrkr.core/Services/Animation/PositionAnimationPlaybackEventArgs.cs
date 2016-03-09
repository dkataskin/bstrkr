using System;

using bstrkr.core;

namespace bstrkr.core.services.animation
{
	public class PositionAnimationPlaybackEventArgs : EventArgs
	{
		public PositionAnimationPlaybackEventArgs(PathSegment segment)
		{
			this.PathSegment = segment;
		}

		public PathSegment PathSegment { get; private set; }
	}
}