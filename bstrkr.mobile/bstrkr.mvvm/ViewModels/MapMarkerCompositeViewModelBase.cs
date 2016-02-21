using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using bstrkr.core.map;
using bstrkr.core.services.resources;
using bstrkr.core.spatial;

using Cirrious.MvvmCross.ViewModels;

namespace bstrkr.mvvm.viewmodels
{
	public abstract class MapMarkerCompositeViewModelBase<TModel> : MvxViewModel where TModel : class
	{
		private readonly ObservableCollection<MapMarkerViewModel> _markers = new ObservableCollection<MapMarkerViewModel>();
		private readonly ReadOnlyObservableCollection<MapMarkerViewModel> _markersReadOnly;

		private MapMarkerSizes _markerSize;

		private GeoLocation _location;
		private bool _isVisible = true;
		private bool _isSelected = false;
		private MapMarkerSelectionStates _selectionState = MapMarkerSelectionStates.NoSelection;

		private TModel _model;

		public MapMarkerCompositeViewModelBase()
		{
			_markersReadOnly = new ReadOnlyObservableCollection<MapMarkerViewModel>(_markers);
		}

		public virtual MapMarkerSizes MarkerSize
		{
			get { return _markerSize; }

			set
			{
				if (_markerSize != value)
				{
					_markerSize = value;
					this.RaisePropertyChanged(() => this.MarkerSize);
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

		public virtual TModel Model
		{
			get { return _model; }
			private set { this.RaiseAndSetIfChanged(ref _model, value, () => this.Model); }
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
					this.RaisePropertyChanged(() => this.IsSelected);
				}
			}
		}

		public ReadOnlyObservableCollection<MapMarkerViewModel> Markers { get { return _markersReadOnly; }}

		public virtual void Setup(TModel model)
		{
			foreach (var marker in this.GetMapMarkerViewModels(model))
			{
				_markers.Add(marker);
			}

			this.Model = model;
		}

		protected abstract IEnumerable<MapMarkerViewModel> GetMapMarkerViewModels(TModel model);
	}
}