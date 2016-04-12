using System;

namespace bstrkr.core.services.animation
{
    public interface IMarkerPositionAnimator
    {
        void Animate(PathSegment segment);

        void Cancel();

        event EventHandler<PositionAnimationPlaybackEventArgs> FinishedPlaying;

        event EventHandler<PositionAnimationValueChangedEventArgs> ValueChanged;
    }
}