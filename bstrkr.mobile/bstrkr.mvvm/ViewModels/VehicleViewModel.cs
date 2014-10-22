using System;

using Cirrious.MvvmCross.ViewModels;

using bstrkr.core;
using bstrkr.core.map;
using bstrkr.core.services.resources;
using bstrkr.core.spatial;
using bstrkr.mvvm.views;

namespace bstrkr.mvvm.viewmodels
{
	public class VehicleViewModel : MapMarkerViewModelBase<Vehicle>
	{
		public VehicleViewModel(IResourceManager resourceManager) : base(resourceManager)
		{
		}

		public override Vehicle Model
		{
			get 
			{
				return base.Model;
			}

			set
			{
				if (base.Model != value)
				{
					base.Model = value;

					this.RaisePropertyChanged(() => this.VehicleId);
					this.RaisePropertyChanged(() => this.VehicleType);
					this.RaisePropertyChanged(() => this.CarPlate);
					this.RaisePropertyChanged(() => this.Location);
				}
			}
		}

		public string VehicleId
		{
			get { return this.Model == null ? string.Empty : this.Model.Id; }
		}

		public VehicleTypes VehicleType
		{
			get { return this.Model == null ? VehicleTypes.Bus : this.Model.Type; }
		}

		public double VehicleHeading
		{
			get 
			{ 
				return this.Model == null ? 0.0 : this.Model.Heading; 
			}

			set
			{
				if (this.Model != null && this.Model.Heading != value)
				{
					this.Model.Heading = value;
					this.RaisePropertyChanged(() => this.VehicleHeading);
				}
			}
		}

		public string CarPlate
		{
			get { return this.Model == null ? string.Empty : this.Model.CarPlate; }
		}

		public override GeoPoint Location
		{
			get 
			{ 
				return this.Model == null ? GeoPoint.Empty : this.Model.Location; 
			}

			set
			{
				if (this.Model != null && !GeoPoint.Equals(this.Model.Location, value))
				{
					this.Model.Location = value;
					this.RaisePropertyChanged(() => this.Location);
				}
			}
		}

		protected override object GetIcon()
		{
			return this.Model == null ? null : _resourceManager.GetVehicleMarker(this.Model.Type, this.MarkerSize);
		}
	}
}