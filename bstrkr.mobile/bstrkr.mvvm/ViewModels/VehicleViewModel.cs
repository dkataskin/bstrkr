﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Threading.Tasks;

using bstrkr.core;
using bstrkr.core.map;
using bstrkr.core.services.animation;
using bstrkr.core.services.resources;
using bstrkr.core.spatial;
using bstrkr.core.utils;
using bstrkr.mvvm.maps;
using bstrkr.mvvm.views;

using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;

namespace bstrkr.mvvm.viewmodels
{
	public class VehicleViewModel : MapMarkerViewModelBase<Vehicle>
	{
		private const float SegmentTravelTime = 15.0f;

		private readonly object _animationLock = new object();
		private readonly ObservableCollection<PathSegment> _path = new ObservableCollection<PathSegment>();
		private readonly ReadOnlyObservableCollection<PathSegment> _pathReadOnly;

		private IMarkerPositionAnimator _positionAnimator;
		private object _titleIcon;
		private long _lastUpdate = 0;
		private GeoPoint _positionAnimation;
		private bool _animateMovement;
		private bool _isInView;
		private bool _isTitleVisible = true;

		public VehicleViewModel(IAppResourceManager resourceManager) : base(resourceManager)
		{
			_pathReadOnly = new ReadOnlyObservableCollection<PathSegment>(_path);
			this.AnimateMovement = Settings.AnimateMarkers;
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

					this.SetIcons(this.ResourceManager);

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
			get { return this.Model?.RouteInfo?.DisplayName; }
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

		public GeoPoint LocationAnimated
		{
			get { return _positionAnimation; }
			set { this.RaiseAndSetIfChanged(ref _positionAnimation, value, () => this.LocationAnimated); }
		}

		public bool AnimateMovement
		{ 
			get { return _animateMovement; } 
			set { this.RaiseAndSetIfChanged(ref _animateMovement, value, () => this.AnimateMovement); } 
		}

		public bool IsInView
		{
			get { return _isInView; }
			set { this.RaiseAndSetIfChanged(ref _isInView, value, () => this.IsInView); }
		}

		public ReadOnlyObservableCollection<PathSegment> Path { get { return _pathReadOnly; } }

		public object TitleIcon
		{
			get { return _titleIcon; }
			set { this.RaiseAndSetIfChanged(ref _titleIcon, value, () => this.TitleIcon); }
		}

		public bool IsTitleVisible 
		{
			get { return _isTitleVisible; }
			set { this.RaiseAndSetIfChanged(ref _isTitleVisible, value, () => this.IsTitleVisible); }
		}

		public IMarkerPositionAnimator PositionAnimator 
		{ 
			get { return _positionAnimator; }
			set
			{
				_positionAnimator = value;
				if (value != null)
				{
					_positionAnimator.FinishedPlaying += this.OnAnimatorFinishedPlaying;
				};
			}
		}

		public void Update(VehicleLocationUpdate update)
		{
			lock(_animationLock)
			{
				if (_lastUpdate > 0)
				{
					var totalTime = SegmentTravelTime;

					if (update.Waypoints != null && update.Waypoints.Waypoints.Any())
					{
						_path.Add(
							new PathSegment 
							{
								Duration = TimeSpan.FromSeconds(update.Waypoints.Waypoints[0].Fraction * totalTime),
								StartLocation = this.Location,
								FinalLocation = update.Waypoints.Waypoints[0].Location
							});

						for (int i = 0; i < update.Waypoints.Waypoints.Count - 1; i++)
						{
							_path.Add(
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
							_path.Add(
								new PathSegment 
								{
									Duration = TimeSpan.FromSeconds(totalTime),
									StartLocation = this.Location,
									FinalLocation = update.Vehicle.Location
								});
						}
					}
				}
			}

			_lastUpdate = update.LastUpdated.Ticks;
			this.SetLocation(update.Vehicle.Location);

			this.ScheduleAnimation();
		}

		public override string ToString()
		{
			return string.Format("[VehicleVM: Id={0}, Type={1}, CarPlate={2}, RouteNumber={3}, Location={4}]", VehicleId, VehicleType, CarPlate, RouteNumber, Location);
		}

		protected override void SetIcons(IAppResourceManager resourceManager)
		{
			this.Icon = resourceManager.GetVehicleMarker(this.VehicleType, this.MarkerSize, this.IsSelected);
			this.TitleIcon = resourceManager.GetVehicleTitleMarker(this.VehicleType, this.Model?.RouteInfo?.RouteNumber);
		}

		private void SetLocation(GeoLocation location)
		{
			if (this.Model != null && !GeoLocation.Equals(this.Model.Location, location))
			{
				this.Model.Location = location;
				this.RaisePropertyChanged(() => this.Location);
			}
		}

		private void ScheduleAnimation()
		{
			lock(_animationLock)
			{
				if (this.Path.Count > 0)
				{
					var segment = this.Path.First();
					if (this.IsInView)
					{
						this.PositionAnimator.Animate(segment);
					}
					else
					{
						Scheduler.Default.Schedule(segment.Duration, () => this.AnimateSegment(segment));
					}
				}
			}
		}

		private void AnimateSegment(PathSegment segment)
		{
			lock(_animationLock)
			{
				this.LocationAnimated = segment.FinalLocation.Position;
				_path.Remove(segment);
			}

			this.ScheduleAnimation();
		}

		private void OnAnimatorFinishedPlaying(object sender, PositionAnimatorEventArgs args)
		{
			lock(_animationLock)
			{
				_path.Remove(args.PathSegment);
			}
		}
	}
}