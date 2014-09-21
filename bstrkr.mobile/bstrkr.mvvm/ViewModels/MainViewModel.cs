using System.Collections.ObjectModel;

using Cirrious.MvvmCross.ViewModels;

using bstrkr.core;
using bstrkr.core.services.location;

namespace bstrkr.mvvm.viewmodels
{
    public class MainViewModel : MvxViewModel
    {
		private readonly ILocationService _locationService;

		private Vehicle _vehicle;

		public MainViewModel(ILocationService locationService)
		{
			_locationService = locationService;
			_locationService.LocationUpdated += OnLocationUpdated;
			_locationService.StartUpdating();

			this.Vehicles = new ObservableCollection<Vehicle>();
		}

		private void OnLocationUpdated(object sender, LocationUpdatedEventArgs args)
		{
			if (_vehicle == null)
			{
				_vehicle = new Vehicle 
				{
					Location = new bstrkr.core.spatial.GeoPoint(args.Latitude, args.Longitude)
				};
				
				this.Vehicles.Add(_vehicle);
			}
		}

		public ObservableCollection<Vehicle> Vehicles { get; private set; }
    }
}