using System.Collections.ObjectModel;

using Cirrious.MvvmCross.ViewModels;

using bstrkr.core;
using bstrkr.core.services.location;
using bstrkr.core.spatial;

namespace bstrkr.mvvm.viewmodels
{
    public class MainViewModel : MvxViewModel
    {
		private readonly ILocationService _locationService;

		private Vehicle _vehicle;
		private GeoPoint _location;

		public MainViewModel(ILocationService locationService)
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
				if (_location != value)
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