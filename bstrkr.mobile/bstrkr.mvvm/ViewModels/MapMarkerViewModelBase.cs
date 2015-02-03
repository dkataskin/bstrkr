using System;

using Cirrious.MvvmCross.ViewModels;

using bstrkr.core.map;
using bstrkr.core.services.resources;
using bstrkr.core.spatial;

namespace bstrkr.mvvm.viewmodels
{
	public abstract class MapMarkerViewModelBase<T> : MvxViewModel where T : class
	{
		protected IAppResourceManager _resourceManager;

		private MapMarkerSizes _markerSize;
		private object _icon;
		private GeoPoint _location;
		private bool _isVisible = true;

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

		public virtual GeoPoint Location
		{
			get 
			{ 
				return _location;
			}

			set
			{
				if (!GeoPoint.Equals(_location, value))
				{
					_location = value;
					this.RaisePropertyChanged(() => this.Location);
				}
			}
		}

		public virtual T Model
		{
			get 
			{ 
				return _model; 
			}

			set
			{
				if (_model != value)
				{
					_model = value;
					this.RaisePropertyChanged(() => this.Model);
				}
			}
		}

		public virtual object Icon
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

		public virtual bool IsVisible
		{
			get
			{
				return _isVisible;
			}

			set
			{
				if (_isVisible != value)
				{
					_isVisible = value;
					this.RaisePropertyChanged(() => this.IsVisible);
				}
			}
		}

		protected abstract object GetIcon();
	}
}