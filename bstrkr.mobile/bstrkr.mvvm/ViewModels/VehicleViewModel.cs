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
			var animList = new List<Tuple<GeoPoint, GeoPoint, int>>();
			var totalTime = TimeSpan.FromSeconds(10).TotalMilliseconds;
			if (waypoints != null && waypoints.Waypoints.Any())
			{
				
				for (int i = 0; i < waypoints.Waypoints.Count - 1; i++)
				{
					animList.Add(
						new Tuple<GeoPoint, GeoPoint, int>(
										waypoints.Waypoints[i].Location,
										waypoints.Waypoints[i+1].Location,
										Convert.ToInt32(waypoints.Waypoints[i + 1].Fraction  * totalTime)));
				}
			}

			animList.Add(
				new Tuple<GeoPoint, GeoPoint, int>(
					waypoints.Waypoints.Last().Location,
					waypoints.Waypoints.Last().Location,
					Convert.ToInt32(waypoints.Waypoints.Last().Fraction  * totalTime)));

			if (!animList.Any())
			{
				this.SetLocation(currentLocation);
			}
			else
			{
				Task.Factory.StartNew(() =>
				{
					var step = 16;
					var interpolator = new SphericalGeoPointInterpolator();
					foreach (var animItem in animList)
					{
						var animTime = 0;
						while (animTime < animItem.Item3)
						{
							Task.Delay(step).Wait();

							var value = interpolator.Interpolate(animTime / animItem.Item3, animItem.Item1, animItem.Item2);
							this.ViewDispatcher.RequestMainThreadAction(() => this.SetLocation(value));
						}
					}
				});
			}
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