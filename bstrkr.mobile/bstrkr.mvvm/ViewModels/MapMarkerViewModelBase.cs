﻿using System;
using System.Collections.Generic;

using bstrkr.core.map;
using bstrkr.core.services.resources;
using bstrkr.core.spatial;

using Cirrious.MvvmCross.ViewModels;

namespace bstrkr.mvvm.viewmodels
{
	public abstract class MapMarkerViewModelBase<T> : MvxViewModel where T : class
	{
		protected IAppResourceManager _resourceManager;

		private MapMarkerSizes _markerSize;
		private object _icon;
		private GeoLocation _location;
		private bool _isVisible = true;
		private bool _isSelected = false;
		private MapMarkerSelectionStates _selectionState = MapMarkerSelectionStates.NoSelection;

		private T _model;

		public MapMarkerViewModelBase(IAppResourceManager resourceManager)
		{
			_resourceManager = resourceManager;
			this.Icon = this.GetIcon();
		}

		public virtual MapMarkerSizes MarkerSize
		{
			get 
			{
				return _markerSize;
			}

			set
			{
				if (_markerSize != value)
				{
					_markerSize = value;
					this.RaisePropertyChanged(() => this.MarkerSize);
					this.Icon = this.GetIcon();
				}
				else if (this.Icon == null)
				{
					this.Icon = this.GetIcon();
				}
			}
		}

		public virtual GeoLocation Location
		{
			get 
			{ 
				return _location;
			}

			set
			{
				if (!GeoLocation.Equals(_location, value))
				{
					_location = value;
					this.RaisePropertyChanged(() => this.Location);
				}
			}
		}

		public virtual T Model
		{
			get { return _model; }
			set { this.RaiseAndSetIfChanged(ref _model, value, () => this.Model); }
		}

		public virtual object Icon
		{
			get { return _icon; }
			private set { this.RaiseAndSetIfChanged(ref _icon, value, () => this.Icon); }
		}

		public virtual bool IsVisible
		{
			get { return _isVisible; }
			set { this.RaiseAndSetIfChanged(ref _isVisible, value, () => this.IsVisible); }
		}

		public virtual MapMarkerSelectionStates SelectionState
		{
			get { return _selectionState; }
			set 
			{ 
				if (_selectionState != value)
				{
					_selectionState = value;
					this.RaisePropertyChanged(() => this.SelectionState);
					this.IsSelected = value == MapMarkerSelectionStates.SelectionSelected;
				}
			}
		}

		public bool IsSelected
		{
			get { return _isSelected; }
			private set 
			{
				if (_isSelected != value)
				{
					_isSelected = value;
					this.Icon = this.GetIcon();
				}
			}
		}

		protected abstract object GetIcon();
	}
}