using System.Collections.ObjectModel;

using Cirrious.MvvmCross.ViewModels;

using bstrkr.core;
using bstrkr.core.config;
using bstrkr.core.interfaces;
using bstrkr.core.services.location;
using bstrkr.core.spatial;
using System.Linq;
using System;

namespace bstrkr.mvvm.viewmodels
{
    public class MainViewModel : MvxViewModel
    {
		private readonly ILocationService _locationService;
		private readonly IConfigManager _configManager;

		private ILiveDataProvider _liveDataProvider;
		private GeoPoint _location = GeoPoint.Empty;

		public MainViewModel(IConfigManager configManager, ILocationService locationService)
		{
			_configManager = configManager;

			_locationService = locationService;
			_locationService.LocationUpdated += OnLocationUpdated;
			_locationService.StartUpdating();

			this.Vehicles = new ObservableCollection<Vehicle>();
		}

		public ObservableCollection<Vehicle> Vehicles { get; private set; }

		public GeoPoint Location 
		{ 
			get 
			{
				return _location;
			}

			private set 
			{
				if (!_location.Equals(value))
				{
					_location = value;
					this.RaisePropertyChanged(() => this.Location);
				}
			}
		}

		private void OnLocationUpdated(object sender, LocationUpdatedEventArgs args)
		{
			this.Location = new GeoPoint(args.Latitude, args.Longitude);

			this.UpdateLiveDataProvider();
		}

		private void UpdateLiveDataProvider()
		{
			if (_liveDataProvider == null && !this.Location.Equals(GeoPoint.Empty))
			{
				var config = _configManager.GetConfig();

				var possibleLocation = config.Locations
											 .Select(x => new Tuple<double, BusTrackerLocation>(
																						this.Location.DistanceTo(new GeoPoint(x.Latitude, x.Longitude)), 
																						x))
											 .OrderBy(x => x.Item1)
											 .First();
			}
		}
    }
}