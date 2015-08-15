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
using bstrkr.mvvm.maps;
using bstrkr.mvvm.viewmodels;
using bstrkr.mvvm.views;

using Cirrious.CrossCore;
using Cirrious.CrossCore.Core;

namespace bstrkr.android.views
{
	public class VehicleMarker : GoogleMapsMarkerBase, IVehicleMarker
	{
		private MapMarkerAnimationRunner _animationRunner;

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
				.SetIcon(this.ViewModel.Icon as BitmapDescriptor)
				.Flat(true)
				.SetAlpha(this.ConvertSelectionStateToAlpha(this.ViewModel.SelectionState))
				.SetRotation(Convert.ToSingle(this.ViewModel.Location.Heading));
		}

		private void OnVMPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			if (this.Marker == null)
			{
				return;
			}

			if (args.PropertyName.Equals("Location") && (!this.ViewModel.AnimateMovement || GeoPoint.Empty.Equals(this.Location)))
			{
				this.Marker.Position = this.ViewModel.Location.ToLatLng();
				this.Marker.Rotation = this.ViewModel.Location.Heading;
			}

			if (args.PropertyName.Equals("Icon"))
			{
				this.Marker.SetIcon(this.ViewModel.Icon as BitmapDescriptor);
			}

			if (args.PropertyName.Equals("SelectionState"))
			{
				this.Marker.Alpha = this.ConvertSelectionStateToAlpha(this.ViewModel.SelectionState);
			}
		}

		private void OnPathUpdated(object sender, VehiclePathUpdatedEventArgs args)
		{
			if (_animationRunner == null && this.Marker == null)
			{
				return;
			}

			if (_animationRunner == null)
			{
				_animationRunner = new MapMarkerAnimationRunner(this.MapView, this.Marker);
				_animationRunner.PositionValueUpdated += (s, a) => this.ViewModel.PositionAnimation = (GeoPoint)a.Value;
			}

			if (this.ViewModel.AnimateMovement)
			{
				_animationRunner.QueueAnimation(args.PathSegments);
			}
			else
			{
				_animationRunner.StopAllAnimations();
			}
		}
	}
}