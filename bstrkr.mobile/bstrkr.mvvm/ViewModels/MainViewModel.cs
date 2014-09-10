using Cirrious.MvvmCross.ViewModels;

using bstrkr.core.services.location;

namespace bstrkr.mvvm.viewmodels
{
    public class MainViewModel : MvxViewModel
    {
		private readonly ILocationService _locationService;

		private string _hello = "Hello MvvmCross";

		public MainViewModel(ILocationService locationService)
		{
			_locationService = locationService;
			_locationService.LocationUpdated += HandleLocationUpdated;
			_locationService.StartUpdating();
		}

		private void HandleLocationUpdated (object sender, LocationUpdatedEventArgs args)
		{
		}

        public string Hello
		{ 
			get 
			{ 
				return this._hello; 
			}

			set 
			{ 
				this._hello = value; 
				this.RaisePropertyChanged(() => this.Hello); 
			}
		}
    }
}