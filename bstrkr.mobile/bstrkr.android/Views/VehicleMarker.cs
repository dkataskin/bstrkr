using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Android.Animation;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Views.Animations;

using bstrkr.core;
using bstrkr.core.android.extensions;
using bstrkr.core.android.views;
using bstrkr.core.spatial;
using bstrkr.core.utils;
using bstrkr.mvvm;
using bstrkr.mvvm.maps;
using bstrkr.mvvm.viewmodels;
using bstrkr.mvvm.views;

using Cirrious.CrossCore;
using Cirrious.CrossCore.Core;

namespace bstrkr.android.views
{
	public class VehicleMarker : GoogleMapsMarkerBase, IVehicleMarker, ICleanable
	{
		private const string VehicleMarkerKey = "vehicle";
		private const string TitleMarkerKey = "title";

		private readonly object _lockObject = new object();

//		private MapMarkerAnimationRunner _animationRunner;

		public VehicleMarker(VehicleViewModel vehicleVM)
		{
			this.ViewModel = vehicleVM;
			this.ViewModel.PropertyChanged += this.OnVMPropertyChanged;
		}

		public VehicleViewModel ViewModel { get; private set; }

		public override IDictionary<string, MarkerOptions> GetOptions()
		{
			return new Dictionary<string, MarkerOptions>
			{
				{ 
					VehicleMarkerKey, 
					new MarkerOptions()
						.Anchor(0.5f, 0.5f)
						.SetPosition(this.ViewModel.Location.ToLatLng())
						.SetTitle(this.ViewModel.RouteNumber)
						.SetIcon(this.ViewModel.Icon as BitmapDescriptor)
						.Flat(true)
						.SetAlpha(this.ConvertSelectionStateToAlpha(this.ViewModel.SelectionState))
						.SetRotation(Convert.ToSingle(this.ViewModel.Location.Heading)) 
				},
				{ 
					TitleMarkerKey, 
					new MarkerOptions()
						.Anchor(0.0f, 0.5f)
						.SetPosition(this.ViewModel.Location.ToLatLng())
						.SetTitle(this.ViewModel.RouteNumber)
						.SetIcon(this.ViewModel.TitleIcon as BitmapDescriptor)
						.Flat(false)
						.SetAlpha(this.ConvertSelectionStateToAlpha(this.ViewModel.SelectionState))
				}
			};
		}

		public void CleanUp()
		{
//			lock(_lockObject)
//			{
//				_animationRunner.StopAllAnimations();
//				_animationRunner.PositionValueUpdated -= this.OnPositionValueUpdated;
//			}
		}

		private void OnVMPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			if (!this.Markers.Any())
			{
				return;
			}

			if (args.PropertyName.Equals("Location") && (!this.ViewModel.AnimateMovement || GeoPoint.Empty.Equals(this.Location)))
			{
				foreach (var marker in this.Markers.Values)
				{
					marker.Position = this.ViewModel.Location.ToLatLng();
					marker.Rotation = this.ViewModel.Location.Heading;
				}
			}

//			if (args.PropertyName.Equals("Icon"))
//			{
//				this.Marker.SetIcon(this.ViewModel.Icon as BitmapDescriptor);
//			}

			if (args.PropertyName.Equals("SelectionState"))
			{
				foreach (var marker in this.Markers.Values)
				{
					marker.Alpha = this.ConvertSelectionStateToAlpha(this.ViewModel.SelectionState);
				}
			}
		}

//		private void OnPathUpdated(object sender, VehiclePathUpdatedEventArgs args)
//		{
//			lock(_lockObject)
//			{
//				if (_animationRunner == null && this.Marker == null)
//				{
//					return;
//				}
//
//				if (_animationRunner == null)
//				{
//					_animationRunner = new MapMarkerAnimationRunner(this.MapView, this.Marker);
//					_animationRunner.PositionValueUpdated += this.OnPositionValueUpdated;
//				}
//
//				if (this.ViewModel.AnimateMovement)
//				{
//					_animationRunner.QueueAnimation(args.PathSegments);
//				}
//				else
//				{
//					_animationRunner.StopAllAnimations();
//				}
//			}
//		}
//
//		private void OnPositionValueUpdated(object sender, AnimationValueUpdatedEventArgs a)
//		{
//			this.ViewModel.LocationAnimated = (GeoPoint)a.Value;
//		}
	}
}