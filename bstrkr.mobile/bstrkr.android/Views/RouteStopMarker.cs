using System;
using System.ComponentModel;

using Android.Gms.Maps.Model;

using bstrkr.core.android.extensions;
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
				.Anchor(0.5f, 0.5f)
				.SetPosition(this.ViewModel.Location.ToLatLng())
				.SetTitle(this.ViewModel.Model.Name)
				.SetSnippet(this.ViewModel.Model.Description)
				.SetIcon(this.ViewModel.Icon as BitmapDescriptor)
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

			if (args.PropertyName.Equals("IsVisible"))
			{
				this.Marker.Visible = this.ViewModel.IsVisible;
			}

			if (args.PropertyName.Equals("SelectionState"))
			{
				this.Marker.Alpha = this.ConvertSelectionStateToAlpha(this.ViewModel.SelectionState);
			}
		}
	}
}