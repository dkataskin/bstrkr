using System;

using Cirrious.MvvmCross.ViewModels;

using bstrkr.core;
using bstrkr.core.services.resources;
using bstrkr.core.spatial;
using bstrkr.mvvm.views;

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
					this.RaisePropertyChanged(() => this.Vehicle);

					if (value != null)
					{
						this.Icon = _resourceManager.GetVehicleMarker(value.Type, 12.0f);
					}

					this.RaisePropertyChanged(() => this.VehicleId);
					this.RaisePropertyChanged(() => this.VehicleType);
					this.RaisePropertyChanged(() => this.CarPlate);
					this.RaisePropertyChanged(() => this.Location);
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
			get 
			{ 
				return _vehicle == null ? 0.0 : _vehicle.Heading; 
			}

			set
			{
				if (_vehicle != null && _vehicle.Heading != value)
				{
					_vehicle.Heading = value;
					this.RaisePropertyChanged(() => this.VehicleHeading);
				}
			}
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
				if (_vehicle != null && !GeoPoint.Equals(_vehicle.Location, value))
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