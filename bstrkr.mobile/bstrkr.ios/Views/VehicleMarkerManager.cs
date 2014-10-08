using System;
using System.Collections;

using Cirrious.MvvmCross.Binding.Attributes;

using Google.Maps;

using MonoTouch.CoreLocation;
using MonoTouch.UIKit;

using bstrkr.core;
using bstrkr.mvvm.maps;
using bstrkr.mvvm.viewmodels;
using bstrkr.mvvm.views;

namespace bstrkr.ios.views
{
	public class VehicleMarkerManager : MvxMarkerManager
	{
		public VehicleMarkerManager(IMapView mapView) : base(mapView)
		{
		}

		[MvxSetToNullAfterBinding]
		public override IEnumerable ItemsSource 
		{
			get { return base.ItemsSource; }
			set { base.ItemsSource = value; }
		}

		protected override IVehicleMarker CreateMarker(object item)
		{
			return new VehicleMarker(item as VehicleViewModel);
		}
	}
}