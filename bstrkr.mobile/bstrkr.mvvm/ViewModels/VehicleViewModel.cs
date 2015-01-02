using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Cirrious.MvvmCross.ViewModels;

using bstrkr.core;
using bstrkr.core.map;
using bstrkr.core.services.resources;
using bstrkr.core.spatial;
using bstrkr.mvvm.views;
using bstrkr.mvvm.maps;

namespace bstrkr.mvvm.viewmodels
{
	public class VehicleViewModel : MapMarkerViewModelBase<Vehicle>
	{
		private readonly ObservableCollection<WaySegment> _waypoints = new ObservableCollection<WaySegment>();

		private long _lastUpdate = 0;

		public VehicleViewModel(IAppResourceManager resourceManager) : base(resourceManager)
		{
			this.WaySegments = new ReadOnlyObservableCollection<WaySegment>(_waypoints);
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
					this.RaisePropertyChanged(() => this.VehicleHeading);
					this.RaisePropertyChanged(() => this.CarPlate);
					this.RaisePropertyChanged(() => this.Location);
					this.RaisePropertyChanged(() => this.Icon);
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

		public float VehicleHeading
		{
			get 
			{ 
				return this.Model == null ? 0.0f : this.Model.Heading; 
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

		public string RouteNumber
		{
			get { return (this.Model == null || this.Model.RouteInfo == null) ? string.Empty : this.Model.RouteInfo.DisplayName; }
		}

		public override GeoPoint Location
		{
			get 
			{ 
				return this.Model == null ? GeoPoint.Empty : this.Model.Location; 
			}

			set
			{
			}
		}

		public ReadOnlyObservableCollection<WaySegment> WaySegments { get; private set; }

		public void UpdateLocation(GeoPoint currentLocation, WaypointCollection waypoints)
		{
			this.SetLocation(currentLocation);
//			if (this.Location.Equals(GeoPoint.Empty) || waypoints == null)
//			{
//				this.SetLocation(currentLocation);
//				_waypoints.Clear();
//			}
//
//			if (waypoints != null)
//			{
//				var sortedWaypoints = waypoints.Waypoints.OrderBy(x => x.Fraction);
//			}
		}

		protected override object GetIcon()
		{
			return this.Model == null ? null : _resourceManager.GetVehicleMarker(this.Model.Type, this.MarkerSize);
		}

		private void SetLocation(GeoPoint location)
		{
			if (this.Model != null && !GeoPoint.Equals(this.Model.Location, location))
			{
				this.Model.Location = location;
				this.RaisePropertyChanged(() => this.Location);
			}
		}
	}
}