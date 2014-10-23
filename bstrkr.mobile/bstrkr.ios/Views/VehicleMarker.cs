using System;
using System.ComponentModel;

using Google.Maps;

using MonoTouch.CoreLocation;
using MonoTouch.UIKit;

using bstrkr.mvvm.viewmodels;
using bstrkr.mvvm.views;
using bstrkr.core.ios.extensions;
using bstrkr.core.spatial;

namespace bstrkr.ios.views
{
	public class VehicleMarker : GoogleMapsMarkerBase, IVehicleMarker
	{
		public VehicleMarker(VehicleViewModel vehicleVM)
		{
			this.ViewModel = vehicleVM;

			this.Position = new CLLocationCoordinate2D(vehicleVM.Location.Latitude, vehicleVM.Location.Longitude);
			this.Flat = true;
			this.Rotation = vehicleVM.VehicleHeading;
			this.Icon = vehicleVM.Icon as UIImage;

			this.ViewModel.PropertyChanged += this.OnVMPropertyChanged;
		}

		public VehicleViewModel ViewModel { get; private set; }

		private void OnVMPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			if (args.PropertyName.Equals("Location"))
			{
				this.Location = this.ViewModel.Location;
				this.Rotation = this.ViewModel.VehicleHeading;
			}

			if (args.PropertyName.Equals("Icon"))
			{
				this.Icon = this.ViewModel.Icon as UIImage;
			}
		}
	}
}