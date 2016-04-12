using System;
using System.Collections.Generic;
using System.Linq;

using Android.Animation;
using Android.Gms.Maps.Model;

using bstrkr.core.android.extensions;
using bstrkr.core.services.animation;
using bstrkr.core.spatial;

namespace bstrkr.core.android.views
{
    public class MarkerPositionAnimationRunner : Java.Lang.Object,
                                                 Animator.IAnimatorListener,
                                                 ValueAnimator.IAnimatorUpdateListener,
    IMarkerPositionAnimator
    {
        private readonly object _lockObject = new object();
        private readonly IEnumerable<Marker> _markers;

        private AnimatorSet _animatorSet = new AnimatorSet();

        private int _count;
        private LatLng _prevPosition;
        private PathSegment _pathSegment;

        public MarkerPositionAnimationRunner(IEnumerable<Marker> markers)
        {
            _markers = markers;
        }

        public bool AnimationRunning
        {
            get
            {
                lock (_lockObject)
                {
                    return _animatorSet.IsRunning;
                }
            }
        }

        public event EventHandler<AnimationValueUpdatedEventArgs> PositionValueUpdated;

        public event EventHandler<PositionAnimationPlaybackEventArgs> FinishedPlaying;

        public void Animate(PathSegment segment)
        {
            lock (_lockObject)
            {
                if (!_animatorSet.IsRunning)
                {
                    _pathSegment = segment;

                    _animatorSet = new AnimatorSet();
                    _animatorSet.AddListener(this);

                    var animators = new List<ObjectAnimator>();
                    foreach (var marker in _markers)
                    {
                        var positionAnimator = ObjectAnimator.OfObject(
                            marker,
                            "Position",
                            new MarkerPositionEvaluator(),
                            segment.FinalLocation.ToLatLng());

                        positionAnimator.AddListener(this);
                        positionAnimator.AddUpdateListener(this);
                        positionAnimator.SetDuration(Convert.ToInt64(segment.Duration.TotalMilliseconds));

                        animators.Add(positionAnimator);
                    }

                    _animatorSet.PlayTogether(animators.ToArray());
                    _animatorSet.Start();
                }
            }
        }

        public event EventHandler<PositionAnimationValueChangedEventArgs> ValueChanged;

        public void Cancel()
        {
            lock (_lockObject)
            {
                _animatorSet?.Cancel();
            }
        }

        public void OnAnimationCancel(Animator animation)
        {
            animation.RemoveAllListeners();
        }

        public void OnAnimationEnd(Animator animation)
        {
            animation.RemoveAllListeners();
            this.RaiseFinishedPlayingEvent(_pathSegment);
        }

        public void OnAnimationRepeat(Animator animation)
        {
        }

        public void OnAnimationStart(Animator animation)
        {
            _prevPosition = _markers.First().Position;
        }

        public void OnAnimationUpdate(ValueAnimator animation)
        {
            _count++;
            if (_count % 4 == 0)
            {
                var curValue = (LatLng)animation.AnimatedValue;

                var deltaX = curValue.Latitude - _prevPosition.Latitude;
                var deltaY = curValue.Longitude - _prevPosition.Longitude;

                var rotation = Convert.ToSingle(Math.Atan2(deltaY, deltaX) * (180 / Math.PI));
                foreach (var marker in _markers)
                {
                    if (marker.Flat)
                    {
                        marker.Rotation = rotation;
                    }
                }

                _prevPosition = curValue;
                this.RaisePositionValueUpdatedEvent(curValue.ToGeoPoint());
            }
        }

        public void StopAllAnimations()
        {
            lock (_lockObject)
            {
                if (_animatorSet != null)
                {
                    _animatorSet.Cancel();
                    _animatorSet.RemoveAllListeners();
                }
            }
        }

        private void RaisePositionValueUpdatedEvent(GeoPoint position)
        {
            this.PositionValueUpdated?.Invoke(this, new AnimationValueUpdatedEventArgs(position));
        }

        private void RaiseFinishedPlayingEvent(PathSegment pathSegment)
        {
            this.FinishedPlaying?.Invoke(this, new PositionAnimationPlaybackEventArgs(pathSegment));
        }
    }
}