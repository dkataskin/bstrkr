using System;
using System.Collections.Concurrent;

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Android.Animation;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;

using bstrkr.core.android.extensions;
using bstrkr.core.spatial;
using bstrkr.mvvm.maps;
using bstrkr.mvvm.viewmodels;
using bstrkr.mvvm.views;
using bstrkr.core.utils;

namespace bstrkr.android.views
{
	public class VehicleMarker : GoogleMapsMarkerBase, IVehicleMarker
	{
		private static IGeoPointInterpolator _geoPointInterpolator = new LinearFixedGeoPointInterpolator();
		private ConcurrentQueue<WaySegment> _animationQueue = new ConcurrentQueue<WaySegment>();
		private long _lastUpdate = 0;
		private float _currentFraction = 0;

		public VehicleMarker(VehicleViewModel vehicleVM)
		{
			this.ViewModel = vehicleVM;
			this.ViewModel.PropertyChanged += this.OnVMPropertyChanged;
			this.ViewModel.PathUpdated += this.OnPathUpdated;
			this.ViewModel.AnimationTimerElapsed += (s, a) => this.Animate();
		}

		public VehicleViewModel ViewModel { get; private set; }

		public override MarkerOptions GetOptions()
		{
			var vehicleType = this.ViewModel.VehicleType.ToString()[0];
			return new MarkerOptions()
				.Anchor(0.5f, 0.5f)
				.SetPosition(new LatLng(this.ViewModel.Location.Latitude, this.ViewModel.Location.Longitude))
				.SetTitle(this.ViewModel.RouteNumber)
				.InvokeIcon(this.ViewModel.Icon as BitmapDescriptor)
				.Flat(true)
				.InvokeRotation(Convert.ToSingle(this.ViewModel.VehicleHeading));
		}

		private void OnVMPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			if (this.Marker == null)
			{
				return;
			}

			if (args.PropertyName.Equals("Location") && _lastUpdate == 0)
			{
				this.Marker.Position = this.ViewModel.Location.ToLatLng();
				this.Marker.Rotation = Convert.ToSingle(this.ViewModel.VehicleHeading);

				if (_lastUpdate == 0)
				{
					_lastUpdate = DateTime.Now.Ticks;
				}
			}

			if (args.PropertyName.Equals("Icon"))
			{
				this.Marker.SetIcon(this.ViewModel.Icon as BitmapDescriptor);
			}

			if (args.PropertyName.Equals("VehicleHeading"))
			{
				this.Marker.Rotation = this.ViewModel.VehicleHeading;
			}
		}

		private void OnPathUpdated(object sender, VehiclePathUpdatedEventArgs args)
		{
			foreach (var pathSegment in args.PathSegments)
			{
				_animationQueue.Enqueue(pathSegment);
			}
		}

		private void Animate()
		{
			var newUpdate = DateTime.Now.Ticks;
			if (_lastUpdate > 0)
			{
				var timeDiff = newUpdate - _lastUpdate;
				WaySegment pathSegment = null;
				if (_animationQueue.TryPeek(out pathSegment))
				{
					var animationLength = pathSegment.Duration.Ticks;
					var animationRunAt = animationLength * _currentFraction + timeDiff;
					var fraction = animationRunAt / animationLength;

					var geoPoint = _geoPointInterpolator.Interpolate(
									               fraction > 1.0f ? 1.0f : fraction,
									               pathSegment.StartPosition,
									               pathSegment.FinalPosition);


					this.Marker.Position = geoPoint.ToLatLng();

					if (fraction > 1.0f)
					{
						_currentFraction = 0.0f;
						_animationQueue.TryDequeue(out pathSegment);
					}
					else
					{
						_currentFraction = fraction;
					}
				}
			}

			_lastUpdate = newUpdate;
		}

		private class TpEvaluator : Java.Lang.Object, ITypeEvaluator
		{
			public Java.Lang.Object Evaluate(float fraction, Java.Lang.Object startValue, Java.Lang.Object endValue)
			{
				LatLng b = endValue as LatLng;
				LatLng a = startValue as LatLng;

				double lat = (b.Latitude - a.Latitude) * fraction + a.Latitude;
				double lngDelta = b.Longitude - a.Longitude;

				// Take the shortest path across the 180th meridian.
				if (Math.Abs(lngDelta) > 180) 
				{
					lngDelta -= Math.Sign(lngDelta) * 360;
				}
				double lng = lngDelta * fraction + a.Longitude;
				return new LatLng(lat, lng);
			}
			
		}
//
//		private class AnimatorRunner : Java.Lang.Object, Android.Animation.Animator.IAnimatorListener
//		{
//			private readonly object _lockObject = new object();
//			private bool _animationRunning;
//			private ConcurrentQueue<ObjectAnimator> _animationQueue = new ConcurrentQueue<ObjectAnimator>();
//			private Marker _marker;
//
//			public AnimatorRunner(Marker marker)
//			{
//				_marker = marker;
//			}
//
//			public void Animate(WaySegment waySegment)
//			{
//				ObjectAnimator animator = ObjectAnimator.OfObject(
//								_marker, 
//								"Position", 
//								new TpEvaluator(),
//								waySegment.FinalPosition.ToLatLng());
//				
//				animator.AddListener(this);
//				animator.SetDuration(Convert.ToInt64(waySegment.Duration.TotalMilliseconds));
//
//				bool runAnimation = false;
//				lock(_lockObject)
//				{
//					runAnimation = !_animationRunning;
//					_animationQueue.Enqueue(animator);
//				}
//
//				if (runAnimation)
//				{
//					this.RunNext();
//				}
//			}
//
//			public void OnAnimationCancel(Animator animation)
//			{
//				lock(_lockObject)
//				{
//					_animationRunning = false;
//				}
//			}
//
//			public void OnAnimationEnd(Animator animation)
//			{
//				lock(_lockObject)
//				{
//					_animationRunning = false;
//				}
//
//				this.RunNext();
//			}
//
//			public void OnAnimationRepeat(Animator animation)
//			{
//			}
//
//			public void OnAnimationStart(Animator animation)
//			{
//				lock(_lockObject)
//				{
//					_animationRunning = true;
//				}
//			}
//
//			private void RunNext()
//			{
//				ObjectAnimator animator = null;
//				_animationQueue.TryDequeue(out animator);
//
//				if (animator != null)
//				{
//					animator.Start();
//				}
//			}
//		}
	}
}