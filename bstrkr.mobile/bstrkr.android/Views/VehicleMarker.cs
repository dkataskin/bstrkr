using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Android.Animation;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;

using bstrkr.core;
using bstrkr.core.android.extensions;
using bstrkr.core.spatial;
using bstrkr.core.utils;
using bstrkr.mvvm.maps;
using bstrkr.mvvm.viewmodels;
using bstrkr.mvvm.views;

using Cirrious.CrossCore;
using Cirrious.CrossCore.Core;
using Android.Views.Animations;

namespace bstrkr.android.views
{
	public class VehicleMarker : GoogleMapsMarkerBase, IVehicleMarker
	{
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
			return new MarkerOptions()
				.Anchor(0.5f, 0.5f)
				.SetPosition(this.ViewModel.Location.ToLatLng())
				.SetTitle(this.ViewModel.RouteNumber)
				.InvokeIcon(this.ViewModel.Icon as BitmapDescriptor)
				.Flat(true)
				.InvokeRotation(Convert.ToSingle(this.ViewModel.Location.Heading));
		}

		private void OnVMPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			if (this.Marker == null)
			{
				return;
			}

			if (args.PropertyName.Equals("Location") && GeoPoint.Empty.Equals(this.Location))
			{
				this.Marker.Position = this.ViewModel.Location.ToLatLng();
				this.Marker.Rotation = this.ViewModel.Location.Heading;
			}

			if (args.PropertyName.Equals("Icon"))
			{
				this.Marker.SetIcon(this.ViewModel.Icon as BitmapDescriptor);
			}
		}

		private void OnPathUpdated(object sender, VehiclePathUpdatedEventArgs args)
		{
			if (_animatorRunner == null && this.Marker == null)
			{
				return;
			}

			if (_animatorRunner == null)
			{
				_animatorRunner = new AnimatorRunner(this.MapView, this.Marker);
			}

			_animatorRunner.QueueAnimation(args.PathSegments);
		}

		private class TpEvaluator : Java.Lang.Object, ITypeEvaluator
		{
			public Java.Lang.Object Evaluate(float fraction, Java.Lang.Object startValue, Java.Lang.Object endValue)
			{
				var b = endValue as LatLng;
				var a = startValue as LatLng;

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
			private readonly Queue<PathSegment> _animationQueue = new Queue<PathSegment>();
			private readonly Marker _marker;
			private readonly IMapView _mapView;

			private AnimatorSet _animatorSet = null;

			public AnimatorRunner(IMapView mapView, Marker marker)
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
						return _animatorSet != null;
					}
				} 
			}

			public void QueueAnimation(IEnumerable<PathSegment> waySegments)
			{
				lock(_lockObject)
				{
					foreach (var waySegment in waySegments)
					{
						_animationQueue.Enqueue(waySegment);
					}

					if (_animatorSet == null)
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

					if (_animatorSet == null)
					{
						this.RunNext();
					}
				}
			}

			public void OnAnimationCancel(Animator animation)
			{
				lock(_lockObject)
				{
					_animatorSet = null;
				}
			}

			public void OnAnimationEnd(Animator animation)
			{
				lock(_lockObject)
				{
					_animatorSet = null;
				}

				this.RunNext();
			}

			public void OnAnimationRepeat(Animator animation)
			{
			}

			public void OnAnimationStart(Animator animation)
			{
			}

			private void RunNext()
			{
				lock(_lockObject)
				{
					if (_animatorSet == null)
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
																new TpEvaluator(),
																pathSegment.StartLocation.ToLatLng(),
																pathSegment.FinalLocation.ToLatLng());
							}
							else
							{
								positionAnimator = ObjectAnimator.OfObject(
																_marker, 
																"Position", 
																new TpEvaluator(),
																pathSegment.FinalLocation.ToLatLng());
							}

							positionAnimator.AddListener(this);
							positionAnimator.SetDuration(Convert.ToInt64(pathSegment.Duration.TotalMilliseconds));

							var rotationAnimator = ObjectAnimator.OfFloat(
										                        _marker,
										                        "Rotation",
										                        pathSegment.FinalLocation.Heading + 90);
							rotationAnimator.AddListener(this);
							rotationAnimator.SetDuration(Convert.ToInt64(pathSegment.Duration.TotalMilliseconds));

							_animatorSet = new AnimatorSet();
							_animatorSet.AddListener(this);
							_animatorSet.Play(positionAnimator).With(rotationAnimator);
							_animatorSet.Start();
						}
						else if (!GeoLocation.Empty.Equals(targetLocation))
						{
							_marker.Position = targetLocation.ToLatLng();
						}
					}
				}
			}
		}
	}
}