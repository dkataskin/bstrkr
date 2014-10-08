using System;

using System.ComponentModel;

using Android.Gms.Maps.Model;

using bstrkr.mvvm.viewmodels;
using bstrkr.mvvm.views;
using Android.Gms.Maps;

namespace Views
{
	public class VehicleMarker : Marker
	{
		private readonly VehicleViewModel _vehicleVM;

		private IMapView _mapView;

		public VehicleMarker(VehicleViewModel vehicleVM)
		{
			_vehicleVM = vehicleVM;

			this.Position = new LatLng(vehicleVM.Location.Latitude, vehicleVM.Location.Longitude);
			this.Flat = true;
			this.Rotation = vehicleVM.VehicleHeading;
			//this.i = vehicleVM.Icon as UIImage;

			_vehicleVM.PropertyChanged += this.OnVMPropertyChanged;
		}

		public IMapView MapView 
		{
			get 
			{ 
				return _mapView; 
			}

			set 
			{ 
				_mapView = value;
			}
		}

		private void OnVMPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			if (args.PropertyName.Equals("Location"))
			{
				this.Position = new LatLng(_vehicleVM.Location.Latitude, _vehicleVM.Location.Longitude);
			}
		}
	}
}