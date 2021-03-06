﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;

using bstrkr.core;
using bstrkr.core.services.animation;
using bstrkr.core.services.resources;
using bstrkr.core.spatial;

using Cirrious.MvvmCross.ViewModels;

namespace bstrkr.mvvm.viewmodels
{
    public class VehicleViewModel : MapMarkerViewModelBase<Vehicle>
    {
        private const float SegmentTravelTime = 15.0f;

        private readonly object _animationLock = new object();
        private readonly Queue<PathSegment> _path = new Queue<PathSegment>();

        private IMarkerPositionAnimator _positionAnimator;
        private object _titleIcon;
        private long _lastUpdate = 0;
        private GeoPoint _locationAnimated;
        private bool _animateMovement;
        private bool _isInView;
        private bool _isTitleVisible = true;
        private bool _isAnimationRunning = false;

        public VehicleViewModel(IAppResourceManager resourceManager) : base(resourceManager)
        {
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

                    this.LocationAnimated = this.Location.Position;
                    this.SetIcons(this.ResourceManager);

                    this.RaisePropertyChanged(() => this.VehicleId);
                    this.RaisePropertyChanged(() => this.VehicleType);
                    this.RaisePropertyChanged(() => this.CarPlate);
                    this.RaisePropertyChanged(() => this.Location);
                    this.RaisePropertyChanged(() => this.LocationAnimated);
                    this.RaisePropertyChanged(() => this.Icon);
                }
            }
        }

        public string VehicleId => this.Model == null ? string.Empty : this.Model.Id;

        public VehicleTypes VehicleType => this.Model?.Type ?? VehicleTypes.Bus;

        public string CarPlate => this.Model == null ? string.Empty : this.Model.CarPlate;

        public string RouteNumber => this.Model?.RouteInfo?.DisplayName;

        public override GeoLocation Location
        {
            get
            {
                return this.Model?.Location ?? GeoLocation.Empty;
            }

            set
            {
            }
        }

        public GeoPoint LocationAnimated
        {
            get { return _locationAnimated; }
            set { this.RaiseAndSetIfChanged(ref _locationAnimated, value, () => this.LocationAnimated); }
        }

        public bool AnimateMovement
        {
            get { return _animateMovement; }
            set
            {
                lock (_animationLock)
                {
                    this.RaiseAndSetIfChanged(ref _animateMovement, value, () => this.AnimateMovement);
                }
            }
        }

        public bool IsInView
        {
            get { return _isInView; }
            set
            {
                if (_isInView != value)
                {
                    _isInView = value;
                    this.RaisePropertyChanged(() => this.IsInView);
                }
            }
        }

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
                    _positionAnimator.ValueChanged += (s, a) => _locationAnimated = a.Value;
                };
            }
        }

        public DateTime LastUpdate => new DateTime(_lastUpdate);

        public void Update(VehicleLocationUpdate update)
        {
            lock (_animationLock)
            {
                if (_lastUpdate > 0)
                {
                    if (update.Waypoints != null && update.Waypoints.Waypoints.Any())
                    {
                        _path.Enqueue(
                            new PathSegment
                            {
                                Duration = TimeSpan.FromSeconds(update.Waypoints.Waypoints[0].Fraction * SegmentTravelTime),
                                StartLocation = this.Location,
                                FinalLocation = update.Waypoints.Waypoints[0].Location
                            });

                        for (var i = 0; i < update.Waypoints.Waypoints.Count - 1; i++)
                        {
                            _path.Enqueue(
                                new PathSegment
                                {
                                    Duration = TimeSpan.FromSeconds(update.Waypoints.Waypoints[i + 1].Fraction * SegmentTravelTime),
                                    StartLocation = update.Waypoints.Waypoints[i].Location,
                                    FinalLocation = update.Waypoints.Waypoints[i + 1].Location
                                });
                        };
                    }
                    else
                    {
                        if (!this.Location.Equals(GeoLocation.Empty))
                        {
                            _path.Enqueue(
                                new PathSegment
                                {
                                    Duration = TimeSpan.FromSeconds(SegmentTravelTime),
                                    StartLocation = this.Location,
                                    FinalLocation = update.Vehicle.Location
                                });
                        }
                    }
                }
            }

            _lastUpdate = update.LastUpdated.Ticks;
            this.RaisePropertyChanged(() => this.LastUpdate);
            this.SetLocation(update.Vehicle.Location);

            this.RunAnimation();
        }

        public override string ToString()
        {
            return $"[VehicleVM: Id={VehicleId}, Type={VehicleType}, CarPlate={CarPlate}, RouteNumber={RouteNumber}, Location={Location}]";
        }

        protected override void SetIcons(IAppResourceManager resourceManager)
        {
            this.Icon = resourceManager.GetVehicleMarker(this.VehicleType, this.MarkerSize, this.IsSelected);
            this.TitleIcon = resourceManager.GetVehicleTitleMarker(this.VehicleType, this.Model?.RouteInfo?.RouteNumber);
        }

        private void SetLocation(GeoLocation location)
        {
            if (this.Model != null && !Equals(this.Model.Location, location))
            {
                this.Model.Location = location;
                this.RaisePropertyChanged(() => this.Location);
            }
        }

        private void RunAnimation()
        {
            lock (_animationLock)
            {
                if (!_isAnimationRunning && _path.Count > 0)
                {
                    var pathSegment = _path.Dequeue();
                    if (this.IsInView && this.AnimateMovement)
                    {
                        this.ViewDispatcher.RequestMainThreadAction(() => this.PositionAnimator.Animate(pathSegment));
                    }
                    else
                    {
                        Scheduler.Default.Schedule(pathSegment.Duration, () => this.AnimateSegment(pathSegment));
                    }

                    _isAnimationRunning = true;
                }
                else if (_path.Count == 0)
                {
                    this.ViewDispatcher.RequestMainThreadAction(() => this.LocationAnimated = this.Location.Position);
                }
            }
        }

        private void AnimateSegment(PathSegment segment)
        {
            lock (_animationLock)
            {
                this.ViewDispatcher.RequestMainThreadAction(() => this.LocationAnimated = segment.FinalLocation.Position);
                _isAnimationRunning = false;
            }

            this.RunAnimation();
        }

        private void OnAnimatorFinishedPlaying(object sender, PositionAnimationPlaybackEventArgs args)
        {
            lock (_animationLock)
            {
                _isAnimationRunning = false;
            }

            this.RunAnimation();
        }
    }
}