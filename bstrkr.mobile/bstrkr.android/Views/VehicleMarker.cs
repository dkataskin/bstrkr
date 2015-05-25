using System;
using System.Collections.Concurrent;

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Android.Animation;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;

using bstrkr.core.android.extensions;
using bstrkr.mvvm.maps;
using bstrkr.mvvm.viewmodels;
using bstrkr.mvvm.views;

namespace bstrkr.android.views
{
	public class VehicleMarker : GoogleMapsMarkerBase, IVehicleMarker
	{
		private bool _locationInitialized;
		private AnimatorRunner _animatorRunner;

		public VehicleMarker(VehicleViewModel vehicleVM)
		{
			this.ViewModel = vehicleVM;
			this.ViewModel.PropertyChanged += this.OnVMPropertyChanged;
			this.ViewModel.PathUpdated += this.OnPathUpdated;
		}

		public VehicleViewModel ViewModel { get; private set; }

		public override MarkerOptions GetOptions()
		{
			var vehicleType = this.ViewModel.VehicleType.ToString()[0];
			return new MarkerOptions()
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

			if (args.PropertyName.Equals("Location") && !_locationInitialized)
			{
				this.Marker.Position = new LatLng(this.ViewModel.Location.Latitude, this.ViewModel.Location.Longitude);
				this.Marker.Rotation = Convert.ToSingle(this.ViewModel.VehicleHeading);

				_locationInitialized = true;
			}

			if (args.PropertyName.Equals("Icon"))
			{
				this.Marker.SetIcon(this.ViewModel.Icon as BitmapDescriptor);
			}
		}

		private void OnPathUpdated(object sender, VehiclePathUpdatedEventArgs args)
		{
			if (_animatorRunner == null)
			{
				_animatorRunner = new AnimatorRunner(this.Marker);
			}

			foreach (var item in args.PathSegments)
			{
				_animatorRunner.Animate(item);
			}
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

		private class AnimatorRunner : Java.Lang.Object, Android.Animation.Animator.IAnimatorListener
		{
			private readonly object _lockObject = new object();
			private bool _animationRunning;
			private ConcurrentQueue<ObjectAnimator> _animationQueue = new ConcurrentQueue<ObjectAnimator>();
			private Marker _marker;

			public AnimatorRunner(Marker marker)
			{
				_marker = marker;
			}

			public void Animate(WaySegment waySegment)
			{
				ObjectAnimator animator = ObjectAnimator.OfObject(
								_marker, 
								"Position", 
								new TpEvaluator(),
								waySegment.FinalPosition.ToLatLng());
				
				animator.AddListener(this);
				animator.SetDuration(Convert.ToInt64(waySegment.Duration.TotalMilliseconds));

				bool runAnimation = false;
				lock(_lockObject)
				{
					runAnimation = !_animationRunning;
					_animationQueue.Enqueue(animator);
				}

				if (runAnimation)
				{
					this.RunNext();
				}
			}

			public void OnAnimationCancel(Animator animation)
			{
				lock(_lockObject)
				{
					_animationRunning = false;
				}
			}

			public void OnAnimationEnd(Animator animation)
			{
				lock(_lockObject)
				{
					_animationRunning = false;
				}

				this.RunNext();
			}

			public void OnAnimationRepeat(Animator animation)
			{
			}

			public void OnAnimationStart(Animator animation)
			{
				lock(_lockObject)
				{
					_animationRunning = true;
				}
			}

			private void RunNext()
			{
				ObjectAnimator animator = null;
				_animationQueue.TryDequeue(out animator);

				if (animator != null)
				{
					animator.Start();
				}
			}
		}
	}
}