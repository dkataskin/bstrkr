﻿using System;
using System.Collections.Generic;

using Android.Animation;
using Android.Gms.Maps.Model;

using bstrkr.core.android.extensions;
using bstrkr.core.spatial;
using bstrkr.mvvm.views;

namespace bstrkr.core.android.views
{
	public class MapMarkerAnimationRunner : Java.Lang.Object, Android.Animation.Animator.IAnimatorListener,
											Android.Animation.ValueAnimator.IAnimatorUpdateListener
	{
		private readonly object _lockObject = new object();
		private readonly Queue<PathSegment> _animationQueue = new Queue<PathSegment>();
		private readonly Marker _marker;
		private readonly IMapView _mapView;

		private AnimatorSet _animatorSet = new AnimatorSet();

		private LatLng _prevPosition;
		private int _count;

		public MapMarkerAnimationRunner(IMapView mapView, Marker marker)
		{
			_mapView = mapView;
			_marker = marker;
		}

		public bool AnimationRunning 
		{ 
			get 
			{ 
				lock(_lockObject)
				{
					return _animatorSet.IsRunning;
				}
			} 
		}

		public event EventHandler<AnimationValueUpdatedEventArgs> PositionValueUpdated;

		public void QueueAnimation(IEnumerable<PathSegment> waySegments)
		{
			lock(_lockObject)
			{
				foreach (var waySegment in waySegments)
				{
					_animationQueue.Enqueue(waySegment);
				}

				if (!_animatorSet.IsRunning)
				{
					this.RunNext();
				}
			}
		}

		public void QueueAnimation(PathSegment waySegment)
		{
			lock(_lockObject)
			{
				_animationQueue.Enqueue(waySegment);

				if (!_animatorSet.IsRunning)
				{
					this.RunNext();
				}
			}
		}

		public void OnAnimationCancel(Animator animation)
		{
			animation.RemoveAllListeners();
		}

		public void OnAnimationEnd(Animator animation)
		{
			animation.RemoveAllListeners();
			this.RunNext();
		}

		public void OnAnimationRepeat(Animator animation)
		{
		}

		public void OnAnimationStart(Animator animation)
		{
			_prevPosition = _marker.Position;
		}

		public void OnAnimationUpdate(ValueAnimator animation)
		{
			_count++;
			if (_count % 4 == 0)
			{
				var curValue = (LatLng)animation.AnimatedValue;

				var deltaX = curValue.Latitude - _prevPosition.Latitude;
				var deltaY = curValue.Longitude - _prevPosition.Longitude;
				_marker.Rotation = Convert.ToSingle(Math.Atan2(deltaY, deltaX) * (180 / Math.PI));
			}

			//this.RaisePositionValueUpdatedEvent(((LatLng)animation.AnimatedValue).ToGeoPoint());
		}

		public void StopAllAnimations()
		{
			_animationQueue.Clear();
			if (_animatorSet != null)
			{
				_animatorSet.Cancel();
				_animatorSet.RemoveAllListeners();
			}
		}

		private void RunNext()
		{
			lock(_lockObject)
			{
				if (!_animatorSet.IsRunning)
				{
					var visibleRegion = _mapView.VisibleRegion;
					var latLngBounds = new LatLngBounds(
						visibleRegion.SouthWest.ToLatLng(),
						visibleRegion.NorthEast.ToLatLng());
					GeoLocation targetLocation = GeoLocation.Empty;
					PathSegment pathSegment = null;
					var animate = false;
					var startPositionIsInView = false;
					var finalPositionIsInView = false;

					while(!animate && _animationQueue.Count > 0)
					{
						pathSegment = _animationQueue.Dequeue();

						startPositionIsInView = latLngBounds.Contains(pathSegment.StartLocation.ToLatLng());
						finalPositionIsInView = latLngBounds.Contains(pathSegment.FinalLocation.ToLatLng());

						animate = startPositionIsInView || finalPositionIsInView;

						targetLocation = pathSegment.FinalLocation;
					}

					if (animate)
					{
						ObjectAnimator positionAnimator = null;
						if (finalPositionIsInView)
						{
							positionAnimator = ObjectAnimator.OfObject(
								_marker, 
								"Position", 
								new MarkerPositionEvaluator(),
								pathSegment.StartLocation.ToLatLng(),
								pathSegment.FinalLocation.ToLatLng());
						}
						else
						{
							positionAnimator = ObjectAnimator.OfObject(
								_marker, 
								"Position", 
								new MarkerPositionEvaluator(),
								pathSegment.FinalLocation.ToLatLng());
						}

						positionAnimator.AddListener(this);
						positionAnimator.AddUpdateListener(this);
						positionAnimator.SetDuration(Convert.ToInt64(pathSegment.Duration.TotalMilliseconds));

						var rotationAnimator = ObjectAnimator.OfFloat(
							_marker,
							"Rotation",
							pathSegment.FinalLocation.Heading + 90);
						rotationAnimator.AddListener(this);
						rotationAnimator.SetDuration(Convert.ToInt64(pathSegment.Duration.TotalMilliseconds));

						_animatorSet = new AnimatorSet();
						_animatorSet.AddListener(this);
						//_animatorSet.Play(positionAnimator).With(rotationAnimator);
						_animatorSet.Play(positionAnimator);
						_animatorSet.Start();
					}
					else if (!GeoLocation.Empty.Equals(targetLocation))
					{
						_marker.Position = targetLocation.ToLatLng();
					}
				}
			}
		}

		private void RaisePositionValueUpdatedEvent(GeoPoint position)
		{
			if (this.PositionValueUpdated != null)
			{
				this.PositionValueUpdated(this, new AnimationValueUpdatedEventArgs(position));
			}
		}
	}
}