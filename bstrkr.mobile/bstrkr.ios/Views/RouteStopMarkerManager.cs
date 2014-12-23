using System;
using System.Collections;

using Cirrious.MvvmCross.Binding.Attributes;

using bstrkr.mvvm.maps;
using bstrkr.mvvm.viewmodels;
using bstrkr.mvvm.views;

namespace bstrkr.ios.views
{
	public class RouteStopMarkerManager : MapMarkerManager
	{
		public RouteStopMarkerManager(IMapView mapView) : base(mapView)
		{
		}

		[MvxSetToNullAfterBinding]
		public override IEnumerable ItemsSource 
		{
			get { return base.ItemsSource; }
			set { base.ItemsSource = value; }
		}

		protected override IMapMarker CreateMarker(object item)
		{
			return new RouteStopMarker(item as RouteStopMapViewModel);
		}
	}
}