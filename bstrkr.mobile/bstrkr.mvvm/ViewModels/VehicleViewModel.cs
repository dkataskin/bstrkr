using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using bstrkr.core;
using bstrkr.core.map;
using bstrkr.core.services.resources;
using bstrkr.core.spatial;
using bstrkr.core.utils;
using bstrkr.mvvm.maps;
using bstrkr.mvvm.views;

using Cirrious.MvvmCross.ViewModels;

namespace bstrkr.mvvm.viewmodels
{
	public class VehicleViewModel : MapMarkerViewModelBase<Vehicle>
	{
		private long _lastUpdate = 0;
		private GeoPoint _positionAnimation;
		private bool _animateMovement;

		public VehicleViewModel(IAppResourceManager resourceManager) : base(resourceManager)
		{
			this.AnimateMovement = Settings.AnimateMarkers;
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

		public string CarPlate
		{
			get { return this.Model == null ? string.Empty : this.Model.CarPlate; }
		}

		public string RouteNumber
		{
			get { return (this.Model == null || this.Model.RouteInfo == null) ? string.Empty : this.Model.RouteInfo.DisplayName; }
		}

		public override GeoLocation Location
		{
			get 
			{ 
				return this.Model == null ? GeoLocation.Empty : this.Model.Location; 
			}

			set
			{
			}
		}

		public GeoPoint PositionAnimation
		{
			get { return _positionAnimation; }
			set { this.RaiseAndSetIfChanged(ref _positionAnimation, value, () => this.PositionAnimation); }
		}

		public bool AnimateMovement 
		{ 
			get { return _animateMovement; } 
			set { this.RaiseAndSetIfChanged(ref _animateMovement, value, () => this.AnimateMovement); } 
		}

		public void UpdateLocation(VehicleLocationUpdate update)
		{
			var animList = new List<PathSegment>();
			if (_lastUpdate > 0)
			{
//				var totalTime = TimeSpan.FromTicks(update.LastUpdated.Ticks - _lastUpdate).TotalSeconds;
				var totalTime = 15.0f;

				if (update.Waypoints != null && update.Waypoints.Waypoints.Any())
				{
					animList.Add(
						new PathSegment 
					{
						Duration = TimeSpan.FromSeconds(update.Waypoints.Waypoints[0].Fraction * totalTime),
						StartLocation = this.Location,
						FinalLocation = update.Waypoints.Waypoints[0].Location
					});

					for (int i = 0; i < update.Waypoints.Waypoints.Count - 1; i++)
					{
						animList.Add(
							new PathSegment 
						{
							Duration = TimeSpan.FromSeconds(update.Waypoints.Waypoints[i + 1].Fraction * totalTime),
							StartLocation = update.Waypoints.Waypoints[i].Location,
							FinalLocation = update.Waypoints.Waypoints[i + 1].Location
						});
					};
				}
				else
				{
					if (!this.Location.Equals(GeoLocation.Empty))
					{
						animList.Add(
							new PathSegment 
							{
								Duration = TimeSpan.FromSeconds(totalTime),
								StartLocation = this.Location,
								FinalLocation = update.Vehicle.Location
							});
					}
				}

				if (animList.Any())
				{
					this.RaisePathUpdatedEvent(animList);
				}
			}

			_lastUpdate = update.LastUpdated.Ticks;
			this.SetLocation(update.Vehicle.Location);
		}

		protected override object GetIcon()
		{
			return this.Model == null ? null : _resourceManager.GetVehicleMarker(this.Model.Type, this.MarkerSize, this.IsSelected);
		}

		private void SetLocation(GeoLocation location)
		{
			if (this.Model != null && !GeoLocation.Equals(this.Model.Location, location))
			{
				this.Model.Location = location;
				this.RaisePropertyChanged(() => this.Location);
			}
		}

		private void RaisePathUpdatedEvent(IList<PathSegment> pathSegments)
		{
			if (this.PathUpdated != null)
			{
				this.PathUpdated(this, new VehiclePathUpdatedEventArgs { PathSegments = pathSegments });
			}
		}
	}
}