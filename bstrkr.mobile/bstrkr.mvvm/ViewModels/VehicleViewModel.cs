using System;

using System.Collections.Generic;
using System.Linq;

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
		private readonly Queue<Tuple<double, GeoPoint>> _queue = new Queue<Tuple<double, GeoPoint>>();

		private long _lastUpdate = 0;

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

		public void AddWaypoints(WaypointCollection waypoints)
		{
			var now = DateTime.UtcNow.Ticks;
			var sortedWaypoints = waypoints.Waypoints.OrderBy(x => x.Fraction);
			lock(_queue)
			{
				_queue.Enqueue(
					new Tuple<double, GeoPoint>(
						TimeSpan.FromTicks(now - _lastUpdate).TotalMilliseconds, 
						sortedWaypoints.First().Location));

				foreach (var waypoint in sortedWaypoints.Skip(1))
				{
					_queue.Enqueue(
						new Tuple<double, GeoPoint>(
							waypoints.TimeSpan.TotalMilliseconds * waypoint.Fraction, 
							waypoint.Location));
				}
			}

			_lastUpdate = now;
		}

		protected override object GetIcon()
		{
			return this.Model == null ? null : _resourceManager.GetVehicleMarker(this.Model.Type, this.MarkerSize);
		}
	}
}