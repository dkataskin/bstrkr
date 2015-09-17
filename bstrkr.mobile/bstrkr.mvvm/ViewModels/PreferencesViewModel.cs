using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using bstrkr.core;
using bstrkr.core.config;
using bstrkr.core.services.location;
using bstrkr.mvvm.messages;
using bstrkr.providers;

using Cirrious.MvvmCross.Plugins.Messenger;
using Cirrious.MvvmCross.ViewModels;

namespace bstrkr.mvvm.viewmodels
{
	public class PreferencesViewModel : BusTrackerViewModelBase
	{
		private readonly IList<Area> _areas = new List<Area>();
		private readonly IAreaPositioningService _areaPositioningService;
		private readonly IConfigManager _configManager;
		private readonly IMvxMessenger _messenger;

		private string _selectedAreaIdOldValue = string.Empty;
		private Area _selectedArea;
		private bool _animateMarkers;

		public PreferencesViewModel(IConfigManager configManager, 
									IAreaPositioningService areaPositioning,
									IMvxMessenger messenger)
		{
			_configManager = configManager;
			_messenger = messenger;
			_areaPositioningService = areaPositioning;

			this.SavePreferencesCommand = new MvxCommand(this.SavePreferences);
		}

		public MvxCommand SavePreferencesCommand { get; private set; }

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

		public override void Start()
		{
			base.Start();

			var config = _configManager.GetConfig();
			foreach (var area in config.Areas)
			{
				_areas.Add(area);
			}

			this.Areas = new ReadOnlyObservableCollection<Area>(new ObservableCollection<Area>(_areas));
			this.SelectedArea = _areaPositioningService.Area;
			this.AnimateMarkers = Settings.AnimateMarkers;

			_selectedAreaIdOldValue = this.SelectedArea == null ? string.Empty : this.SelectedArea.Id;
		}

		private void SavePreferences()
		{
			string areaIdOldValue = Settings.SelectedAreaId;
			Settings.AnimateMarkers = this.AnimateMarkers;
			var selectedAreaId = this.SelectedArea == null ? string.Empty : this.SelectedArea.Id;
			Settings.SelectedAreaId = selectedAreaId;

			if (_selectedAreaIdOldValue != selectedAreaId)
			{
				_areaPositioningService.SelectArea(this.SelectedArea);
			}

//			_messenger.Publish<PreferencesChangedMessage>(
//								new PreferencesChangedMessage(
//														this,
//														this.AnimateMarkers,
//														this.SelectedArea));
		}
	}
}