using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Android.Animation;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Graphics;
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

		public VehicleMarker(VehicleViewModel vehicleVM)
		{
			this.ViewModel = vehicleVM;
			this.ViewModel.PropertyChanged += this.OnVMPropertyChanged;
			this.ViewModel.PositionAnimator = new MarkerPositionAnimationRunner(this.MarkersFlat);
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
						.Anchor(0.5f, 1.0f)
						.SetPosition(this.ViewModel.Location.ToLatLng())
						.SetTitle(this.ViewModel.RouteNumber)
						.SetIcon(BitmapDescriptorFactory.FromBitmap(this.ViewModel.TitleIcon as Bitmap))
						.Visible(this.ViewModel.IsTitleVisible)
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

			if (args.PropertyName.Equals("LocationAnimated") && GeoPoint.Empty.Equals(this.Location))
			{
				foreach (var marker in this.Markers.Values)
				{
					marker.Position = this.ViewModel.Location.ToLatLng();
					if (marker.Flat)
					{
						marker.Rotation = this.ViewModel.Location.Heading;
					}
				}
			}

			if (args.PropertyName.Equals("Icon"))
			{
				this.UpdateIcon(VehicleMarkerKey, this.ViewModel.Icon as BitmapDescriptor);
			}

			if (args.PropertyName.Equals("TitleIcon"))
			{
				this.UpdateIcon(TitleMarkerKey, BitmapDescriptorFactory.FromBitmap(this.ViewModel.Icon as Bitmap));
			}

			if (args.PropertyName.Equals("IsTitleVisible"))
			{
				var marker = this.TryGetMarker(TitleMarkerKey);
				if (marker != null)
				{
					marker.Visible = this.ViewModel.IsTitleVisible;	
				}
			}

			if (args.PropertyName.Equals("SelectionState"))
			{
				foreach (var marker in this.Markers.Values)
				{
					marker.Alpha = this.ConvertSelectionStateToAlpha(this.ViewModel.SelectionState);
				}
			}
		}
	}
}