using System.Drawing;

using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Touch.Views;

using Google.Maps;

using MonoTouch.CoreLocation;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using MonoTouch.UIKit;

using bstrkr.mvvm.viewmodels;

namespace bstrkr.ios.views
{
    [Register("MainView")]
    public class MainView : MvxViewController
    {
		private MapView _mapView;
		private VehicleMarkerManager _manager;

		public override void LoadView()
		{
			base.LoadView();

			CameraPosition camera = CameraPosition.FromCamera (latitude: 54.1815305, 
																longitude: 45.1812519, 
																zoom: 17);
			_mapView = MapView.FromCamera(RectangleF.Empty, camera);
			_mapView.MyLocationEnabled = true;

			View = _mapView;

			_manager = new VehicleMarkerManager(_mapView);

			var set = this.CreateBindingSet<MainView, MainViewModel>();
			set.Bind(_manager).For(m => m.ItemsSource).To(vm => vm.Vehicles);
			set.Apply();

			'//'this.CreateBinding(_mapView).For(m => m.Camera.)
			//var anotherSet = this.CreateBinding<MapView>(_mapView);
			//anotherSet.Bind<MainViewModel>()
		}

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);
			_mapView.StartRendering();
		}

		public override void ViewDidDisappear(bool animated)
		{
			_mapView.StopRendering();
			base.ViewDidDisappear(animated);
		}
    }
}