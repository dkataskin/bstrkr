using System.Drawing;

using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Touch.Views;

using Google.Maps;

using MonoTouch.CoreLocation;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using MonoTouch.UIKit;

using bstrkr.mvvm.maps;
using bstrkr.mvvm.viewmodels;
using bstrkr.mvvm.views;

namespace bstrkr.ios.views
{
    [Register("MainView")]
    public class MainView : MvxViewController
    {
		private IMapView _mapViewWrapper;
		private MapView _mapView;
		private VehicleMarkerManager _vehicleMakerManager;
		private MapLocationManager _mapLocationManager;

		public override void LoadView()
		{
			base.LoadView();

			var camera = CameraPosition.FromCamera (latitude: 54.1815305, 
													longitude: 45.1812519, 
													zoom: 14.0f);

			_mapView = MapView.FromCamera(RectangleF.Empty, camera);
			_mapView.MyLocationEnabled = true;

			_mapViewWrapper = new MonoTouchGoogleMapsView(_mapView);

			this.View = _mapView;

			_vehicleMakerManager = new VehicleMarkerManager(_mapViewWrapper);
			_mapLocationManager = new MapLocationManager(_mapViewWrapper);

			var set = this.CreateBindingSet<MainView, MainViewModel>();
			set.Bind(_vehicleMakerManager).For(m => m.ItemsSource).To(vm => vm.Vehicles);
			set.Bind(_mapLocationManager).For(m => m.Location).To(vm => vm.Location);
			set.Apply();
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