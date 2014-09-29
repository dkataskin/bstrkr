using System;
using System.Collections.ObjectModel;
using System.Linq;

using Cirrious.MvvmCross.ViewModels;

using bstrkr.core;
using bstrkr.core.config;
using bstrkr.core.consts;
using bstrkr.core.interfaces;
using bstrkr.core.providers.bus13;
using bstrkr.core.services.location;
using bstrkr.core.spatial;

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
			this.Location = args.Location;

			this.SelectLiveDataProvider();
		}

		private void SelectLiveDataProvider()
		{
			if (_liveDataProvider == null && !this.Location.Equals(GeoPoint.Empty))
			{
				var config = _configManager.GetConfig();

				var location = config.Locations
									 .Select(x => new Tuple<double, BusTrackerLocation>(
																				this.Location.DistanceTo(new GeoPoint(x.Latitude, x.Longitude)), 
																				x))
									 .OrderBy(x => x.Item1)
									 .First();

				if (location.Item1 <= AppConsts.MaxDistance)
				{
					_liveDataProvider = new Bus13LiveDataProvider(
														location.Item2.Endpoint, 
														location.Item2.LocationId);
				}
				else
				{
					this.OnLocationUnknown();
				}
			}
		}

		private void OnLocationUnknown()
		{
		}
    }
}