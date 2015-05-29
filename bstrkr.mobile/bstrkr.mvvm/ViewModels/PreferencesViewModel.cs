using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using bstrkr.core;
using bstrkr.core.config;
using bstrkr.providers;

using Cirrious.MvvmCross.ViewModels;

namespace bstrkr.mvvm.viewmodels
{
	public class PreferencesViewModel : BusTrackerViewModelBase
	{
		private readonly IList<Area> _areas = new List<Area>();
		private readonly ILiveDataProviderFactory _providerFactory;

		private Area _selectedArea;
		private bool _animateMarkers;

		public PreferencesViewModel(IConfigManager configManager, ILiveDataProviderFactory providerFactory)
		{
			_providerFactory = providerFactory;

			var config = configManager.GetConfig();
			foreach (var area in config.Areas)
			{
				_areas.Add(area);
			}

			this.Areas = new ReadOnlyObservableCollection<Area>(new ObservableCollection<Area>(_areas));

			var provider = _providerFactory.GetCurrentProvider();
			if (provider != null)
			{
				this.SelectedArea = provider.Area;
			}
		}

		public ReadOnlyObservableCollection<Area> Areas { get; private set; }

		public Area SelectedArea 
		{ 
			get { return _selectedArea; }
			set { this.RaiseAndSetIfChanged(ref _selectedArea, value, () => this.SelectedArea); } 
		}

		public bool AnimateMarkers 
		{ 
			get { return _animateMarkers; }
			set { this.RaiseAndSetIfChanged(ref _animateMarkers, value, () => this.AnimateMarkers); }
		}
	}
}