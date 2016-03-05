using System;

using bstrkr.core.spatial;

namespace bstrkr.core.services.animation
{
	public interface IMarkerPositionAnimator
	{
		void Animate(PathSegment segment);

		event EventHandler<PositionAnimatorEventArgs> FinishedPlaying;
	}
}