using System.Drawing;

using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Touch.Views;

using Google.Maps;

using MonoTouch.CoreLocation;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using MonoTouch.UIKit;

using bstrkr.mvvm.viewmodels;

namespace bstrkr.ios.Views
{
    [Register("MainView")]
    public class MainView : MvxViewController
    {
		private MapView _mapView;

		public override void LoadView()
		{
			base.LoadView();

			CameraPosition camera = CameraPosition.FromCamera (latitude: 54.1815305, 
																longitude: 45.1812519, 
																zoom: 17);
			_mapView = MapView.FromCamera(RectangleF.Empty, camera);
			_mapView.MyLocationEnabled = true;

			CLLocationCoordinate2D coord = new CLLocationCoordinate2D (54.1815305, 45.1812519);
			var marker = Marker.FromPosition (coord);
			marker.Title = string.Format ("Marker 1");
			marker.Map = _mapView;


			View = _mapView;
		}

        public override void ViewDidLoad()
        {
			base.ViewDidLoad();

			/*
            this.View = new UIView(){ BackgroundColor = UIColor.White };
            base.ViewDidLoad();

			// ios7 layout
            if (RespondsToSelector(new Selector("edgesForExtendedLayout")))
			{
				this.EdgesForExtendedLayout = UIRectEdge.None;
			}

			this.Add(_mapView);

            var label = new UILabel(new RectangleF(10, 10, 300, 40));
            this.Add(label);

            var textField = new UITextField(new RectangleF(10, 50, 300, 40));
            Add(textField);

            var set = this.CreateBindingSet<MainView, MainViewModel>();
            set.Bind(label).To(vm => vm.Hello);
            set.Bind(textField).To(vm => vm.Hello);
            set.Apply(); */
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