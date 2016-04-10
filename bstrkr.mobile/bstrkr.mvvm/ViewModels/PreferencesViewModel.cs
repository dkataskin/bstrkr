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
		private bool _animateMarkers;

		public PreferencesViewModel()
		{
			this.SavePreferencesCommand = new MvxCommand(this.SavePreferences);
		}

		public MvxCommand SavePreferencesCommand { get; private set; }

		public bool AnimateMarkers 
		{ 
			get { return _animateMarkers; }
			set { this.RaiseAndSetIfChanged(ref _animateMarkers, value, () => this.AnimateMarkers); }
		}

		public override void Start()
		{
			base.Start();

			this.AnimateMarkers = Settings.AnimateMarkers;
		}

		private void SavePreferences()
		{
			Settings.AnimateMarkers = this.AnimateMarkers;
		}
	}
}