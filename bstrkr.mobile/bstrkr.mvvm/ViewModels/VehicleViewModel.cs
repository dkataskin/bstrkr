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
using bstrkr.core.utils;
using System.Threading.Tasks;

namespace bstrkr.mvvm.viewmodels
{
	public class VehicleViewModel : MapMarkerViewModelBase<Vehicle>
	{
		private long _lastUpdate = 0;

		public VehicleViewModel(IAppResourceManager resourceManager) : base(resourceManager)
		{
		}

		public event EventHandler<VehiclePathUpdatedEventArgs> PathUpdated;

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

		public void UpdateLocation(GeoPoint currentLocation, WaypointCollection waypoints)
		{
			var animList = new List<WaySegment>();
			var totalTime = 10.0d;
			if (_lastUpdate > 0)
			{
				totalTime = TimeSpan.FromTicks(DateTime.Now.Ticks - _lastUpdate).TotalSeconds;
			}

			_lastUpdate = DateTime.Now.Ticks;

			if (waypoints != null && waypoints.Waypoints.Any())
			{
				animList.Add(new WaySegment {
					Duration = TimeSpan.FromSeconds(waypoints.Waypoints[0].Fraction * totalTime),
					StartPosition = this.Location,
					FinalPosition = waypoints.Waypoints[0].Location
				});
				
				for (int i = 0; i < waypoints.Waypoints.Count - 1; i++)
				{
					animList.Add(
						new WaySegment {
							Duration = TimeSpan.FromSeconds(waypoints.Waypoints[i + 1].Fraction * totalTime),
							StartPosition = waypoints.Waypoints[i].Location,
							FinalPosition = waypoints.Waypoints[i + 1].Location
						});
				};

//				animList.Add(
//					new WaySegment {
//					Duration = TimeSpan.FromSeconds(waypoints.Waypoints.Last().Fraction * totalTime),
//					StartPosition = waypoints.Waypoints.Last().Location,
//					FinalPosition = currentLocation
//				});
			}
			else
			{
				if (this.Location.Latitude != 0 && this.Location.Longitude != 0)
				{
					animList.Add(
						new WaySegment {
							Duration = TimeSpan.FromSeconds(10),
						StartPosition = this.Location,
						FinalPosition = currentLocation
					});
				}
			}

			if (animList.Any())
			{
				this.RaisePathUpdatedEvent(animList);
			}

			this.SetLocation(currentLocation);
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

		private void RaisePathUpdatedEvent(IList<WaySegment> pathSegments)
		{
			if (this.PathUpdated != null)
			{
				this.PathUpdated(this, new VehiclePathUpdatedEventArgs { PathSegments = pathSegments });
			}
		}
	}

	public class VehiclePathUpdatedEventArgs : EventArgs
	{
		public IList<WaySegment> PathSegments { get; set; }
	}
}