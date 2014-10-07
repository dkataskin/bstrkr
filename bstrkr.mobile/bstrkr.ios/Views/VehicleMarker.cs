﻿using System;

using Google.Maps;

using MonoTouch.CoreLocation;
using MonoTouch.UIKit;

using bstrkr.mvvm.viewmodels;

namespace bstrkr.ios.views
{
	public class VehicleMarker : Marker
	{
		private readonly VehicleViewModel _vehicleVM;

		public VehicleMarker(VehicleViewModel vehicleVM)
		{
			_vehicleVM = vehicleVM;

			this.Position = new CLLocationCoordinate2D(vehicleVM.Location.Latitude, vehicleVM.Location.Longitude);
			this.Flat = true;
			this.Rotation = vehicleVM.VehicleHeading;
			this.Icon = vehicleVM.Icon as UIImage;

			_vehicleVM.PropertyChanged += this.OnVMPropertyChanged;
		}

		private void OnVMPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs args)
		{
			if (args.PropertyName.Equals("Location"))
			{
				this.Position = new CLLocationCoordinate2D(_vehicleVM.Location.Latitude, _vehicleVM.Location.Longitude);
			}
		}
	}
}