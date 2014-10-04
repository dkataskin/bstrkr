using System;
using System.Collections.ObjectModel;
using System.Linq;

using Cirrious.MvvmCross.ViewModels;

using bstrkr.core;
using bstrkr.core.collections;
using bstrkr.core.config;
using bstrkr.core.consts;
using bstrkr.core.interfaces;
using bstrkr.core.providers.bus13;
using bstrkr.core.services.location;
using bstrkr.core.spatial;
using System.Diagnostics;

namespace bstrkr.mvvm.viewmodels
{
    public class MainViewModel : MvxViewModel
    {
		private const int UpdateInterval = 1000;

		private readonly ILocationService _locationService;
		private readonly IConfigManager _configManager;

		private ILiveDataProvider _liveDataProvider;
		private GeoPoint _location = GeoPoint.Empty;

		private BusTrackerLocation _coarseLocation;

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

		public BusTrackerLocation CoarseLocation
		{
			get 
			{ 
				return _coarseLocation; 
			}

			private set 
			{
				if (_coarseLocation != value)
				{
					_coarseLocation = value;
					this.RaisePropertyChanged(() => this.CoarseLocation);
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
					this.CoarseLocation = location.Item2;

					_liveDataProvider = new Bus13LiveDataProvider(
														location.Item2.Endpoint, 
														location.Item2.LocationId,
														TimeSpan.FromMilliseconds(UpdateInterval));
					_liveDataProvider.VehicleLocationsUpdated += this.OnVehicleLocationsUpdated;
					_liveDataProvider.Start();
				}
				else
				{
					this.OnLocationUnknown();
				}
			}
		}

		private void OnVehicleLocationsUpdated(object sender, VehicleLocationsUpdatedEventArgs args)
		{
			try
			{
				this.Dispatcher.RequestMainThreadAction(() =>
												this.Vehicles.Merge(
													args.Vehicles, 
													x => x.Id, 
													(vehicle, update) => vehicle.Location = update.Location,
													MergeMode.Update));
			} 
			catch (Exception e)
			{
				Debug.WriteLine(e.ToString());
			}
		}

		private void OnLocationUnknown()
		{
		}
    }
}