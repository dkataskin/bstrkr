using System.Collections.ObjectModel;

using Cirrious.MvvmCross.ViewModels;

using bstrkr.core;
using bstrkr.core.services.location;
using bstrkr.core.spatial;
using bstrkr.core.config;

namespace bstrkr.mvvm.viewmodels
{
    public class MainViewModel : MvxViewModel
    {
		private readonly ILocationService _locationService;

		private GeoPoint _location;

		public MainViewModel(IConfigManager configManager, ILocationService locationService)
		{
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
		}
    }
}