using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

using Google.Maps;
using MonoTouch.CoreLocation;

namespace bstrkr.ios
{
	public partial class bstrkr_iosViewController : UIViewController
	{
		private MapView _mapView;

		static bool UserInterfaceIdiomIsPhone {
			get { return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone; }
		}

		public bstrkr_iosViewController (IntPtr handle) : base (handle)
		{
		}

		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}

		#region View lifecycle

		public override void LoadView ()
		{
			base.LoadView ();

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

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			// Perform any additional setup after loading the view, typically from a nib.
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear(animated);
			_mapView.StartRendering();
		}

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
		}

		public override void ViewWillDisappear (bool animated)
		{
			base.ViewWillDisappear (animated);
		}

		public override void ViewDidDisappear (bool animated)
		{
			_mapView.StopRendering();
			base.ViewWillDisappear(animated);
		}

		#endregion
	}
}

