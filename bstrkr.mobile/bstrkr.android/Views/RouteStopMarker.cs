using System;
using System.ComponentModel;

using Android.Gms.Maps.Model;

using bstrkr.mvvm.viewmodels;
using bstrkr.mvvm.views;

namespace bstrkr.android.views
{
	public class RouteStopMarker : GoogleMapsMarkerBase, IRouteStopMarker
	{
		public RouteStopMarker(RouteStopMapViewModel routeStopVM)
		{
			this.ViewModel = routeStopVM;
			this.ViewModel.PropertyChanged += this.OnVMPropertyChanged;
		}

		public RouteStopMapViewModel ViewModel { get; private set; }

		public override MarkerOptions GetOptions()
		{
			return new MarkerOptions()
				.SetPosition(new LatLng(this.ViewModel.Location.Latitude, this.ViewModel.Location.Longitude))
				.SetTitle(this.ViewModel.Model.Name)
				.InvokeIcon(this.ViewModel.Icon as BitmapDescriptor)
				.Flat(false);
		}

		private void OnVMPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			if (this.Marker == null)
			{
				return;
			}

			if (args.PropertyName.Equals("Icon"))
			{
				this.Marker.SetIcon(this.ViewModel.Icon as BitmapDescriptor);
			}
		}
	}
}