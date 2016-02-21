using System;

using bstrkr.core.services.resources;

using Cirrious.MvvmCross.ViewModels;
using bstrkr.core.map;

namespace bstrkr.mvvm.viewmodels
{
	public abstract class MapMarkerViewModel : MvxViewModel
	{
		private readonly IAppResourceManager _resourceManager;

		private object _icon;
		private MapMarkerSizes _size;
		private bool _isVisible = true;
		private bool _isSelected = false;
		private MapMarkerSelectionStates _selectionState = MapMarkerSelectionStates.NoSelection;

		public MapMarkerViewModel(IAppResourceManager resourceManager)
		{
			_resourceManager = resourceManager;
		}

		public virtual object Icon
		{
			get { return _icon; }
			private set { this.RaiseAndSetIfChanged(ref _icon, value, () => this.Icon); }
		}

		public virtual MapMarkerSizes Size
		{
			get 
			{
				return _size;
			}

			set
			{
				if (_size != value)
				{
					_size = value;
					this.RaisePropertyChanged(() => this.Size);
					this.Icon = this.GetIcon(_resourceManager);
				}
				else if (this.Icon == null)
				{
					this.Icon = this.GetIcon(_resourceManager);
				}
			}
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
					this.Icon = this.GetIcon(_resourceManager);
				}
			}
		}

		public abstract string Key { get; }

		public virtual void Setup()
		{
			this.Icon = this.GetIcon(_resourceManager);
		}

		protected abstract object GetIcon(IAppResourceManager resourceManager);
	}
}