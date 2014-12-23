using System;
using System.ComponentModel;

using Google.Maps;

using MonoTouch.UIKit;

using bstrkr.mvvm.viewmodels;
using bstrkr.mvvm.views;

namespace bstrkr.ios.views
{
	public class RouteStopMarker : GoogleMapsMarkerBase, IRouteStopMarker
	{
		public RouteStopMarker(RouteStopMapViewModel routeStopVM)
		{
			this.ViewModel = routeStopVM;
			this.Location = routeStopVM.Location;
			this.Icon = routeStopVM.Icon as UIImage;

			routeStopVM.PropertyChanged += this.OnVMPropertyChanged;
		}

		public RouteStopMapViewModel ViewModel { get; private set; }

		private void OnVMPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			if (args.PropertyName.Equals("Icon"))
			{
				this.Icon = this.ViewModel.Icon as UIImage;
			}
		}
	}
}