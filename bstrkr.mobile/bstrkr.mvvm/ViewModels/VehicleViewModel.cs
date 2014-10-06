﻿using System;

using Cirrious.MvvmCross.ViewModels;

using bstrkr.core;
using bstrkr.core.spatial;
using bstrkr.core.services.resources;

namespace bstrkr.mvvm.viewmodels
{
	public class VehicleViewModel : MvxViewModel
	{
		private readonly IResourceManager _resourceManager;

		private object _icon;
		private Vehicle _vehicle;

		public VehicleViewModel(IResourceManager resourceManager)
		{
			_resourceManager = resourceManager;
		}

		public Vehicle Vehicle
		{
			get 
			{
				return _vehicle;
			}

			set
			{
				if (_vehicle != value)
				{
					_vehicle = value;
					this.RaisePropertyChanged(() => Vehicle);

					if (value != null)
					{
						this.Icon = _resourceManager.GetVehicleMarker(value.Type);
					}

					this.RaisePropertyChanged(() => VehicleId);
					this.RaisePropertyChanged(() => VehicleType);
					this.RaisePropertyChanged(() => CarPlate);
					this.RaisePropertyChanged(() => Location);

				}
			}
		}

		public string VehicleId
		{
			get { return _vehicle == null ? string.Empty : _vehicle.Id; }
		}

		public VehicleTypes VehicleType
		{
			get { return _vehicle == null ? VehicleTypes.Bus : _vehicle.Type; }
		}

		public double VehicleHeading
		{
			get { return _vehicle == null ? 0.0 : _vehicle.Heading;}
		}

		public string CarPlate
		{
			get { return _vehicle == null ? string.Empty : _vehicle.CarPlate; }
		}

		public GeoPoint Location
		{
			get 
			{ 
				return _vehicle == null ? GeoPoint.Empty : _vehicle.Location; 
			}

			set
			{
				if (_vehicle != null)
				{
					_vehicle.Location = value;
					this.RaisePropertyChanged(() => this.Location);
				}
			}
		}

		public object Icon
		{
			get 
			{ 
				return _icon;
			}

			private set 
			{
				if (_icon != value)
				{
					_icon = value;
					this.RaisePropertyChanged(() => this.Icon);
				}
			}
		}
	}
}