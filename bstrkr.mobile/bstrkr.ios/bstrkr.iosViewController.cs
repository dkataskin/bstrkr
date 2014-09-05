using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Google.Maps;

namespace bstrkr.ios
{
	public partial class bstrkr_iosViewController : UIViewController
	{
		private MapView mapView;

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

			CameraPosition camera = CameraPosition.FromCamera (latitude: 37.797865, 
																longitude: -122.402526, 
																zoom: 6);
			mapView = MapView.FromCamera (RectangleF.Empty, camera);
			mapView.MyLocationEnabled = true;

			View = mapView;
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			// Perform any additional setup after loading the view, typically from a nib.
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear(animated);
			mapView.StartRendering();
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
			mapView.StopRendering();
			base.ViewWillDisappear(animated);
		}

		#endregion
	}
}

