﻿using System;
using System.ComponentModel;

using Google.Maps;

using MonoTouch.CoreLocation;
using MonoTouch.UIKit;

using bstrkr.mvvm.viewmodels;
using bstrkr.mvvm.views;

namespace bstrkr.ios.views
{
	public class VehicleMarker : Marker, IVehicleMarker
	{
		private readonly VehicleViewModel _vehicleVM;

		private IMapView _mapView;

		public VehicleMarker(VehicleViewModel vehicleVM)
		{
			_vehicleVM = vehicleVM;

			this.Position = new CLLocationCoordinate2D(vehicleVM.Location.Latitude, vehicleVM.Location.Longitude);
			this.Flat = true;
			this.Rotation = vehicleVM.VehicleHeading;
			this.Icon = vehicleVM.Icon as UIImage;

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

				this.Map = value == null ? null : value.MapObject as MapView;
			}
		}

		private void OnVMPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			if (args.PropertyName.Equals("Location"))
			{
				this.Position = new CLLocationCoordinate2D(_vehicleVM.Location.Latitude, _vehicleVM.Location.Longitude);
			}
		}
	}
}