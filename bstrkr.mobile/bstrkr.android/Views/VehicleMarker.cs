﻿using System;

using System.ComponentModel;

using Android.Gms.Maps;
using Android.Gms.Maps.Model;

using bstrkr.mvvm.viewmodels;
using bstrkr.mvvm.views;

namespace bstrkr.android.views
{
	public class VehicleMarker : GoogleMapsMarkerBase, IVehicleMarker
	{
		public VehicleMarker(VehicleViewModel vehicleVM)
		{
			this.ViewModel = vehicleVM;
			this.ViewModel.PropertyChanged += this.OnVMPropertyChanged;
		}

		public VehicleViewModel ViewModel { get; private set; }

		public override MarkerOptions GetOptions()
		{
			return new MarkerOptions()
				.SetPosition(new LatLng(this.ViewModel.Location.Latitude, this.ViewModel.Location.Longitude))
				.Flat(true)
				.InvokeRotation(Convert.ToSingle(this.ViewModel.VehicleHeading));
		}

		private void OnVMPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			if (args.PropertyName.Equals("Location") && this.Marker != null)
			{
				this.Marker.Position = new LatLng(this.ViewModel.Location.Latitude, this.ViewModel.Location.Longitude);
				this.Marker.Rotation = Convert.ToSingle(this.ViewModel.VehicleHeading);
			}
		}
	}
}