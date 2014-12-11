using System;

using Cirrious.MvvmCross.ViewModels;

using bstrkr.core.services.resources;
using bstrkr.core.map;
using bstrkr.core;
using bstrkr.core.spatial;

namespace bstrkr.mvvm.viewmodels
{
	public class RouteStopViewModel : MapMarkerViewModelBase<RouteStop>
	{
		public RouteStopViewModel(IAppResourceManager resourceManager) : base(resourceManager)
		{
		}

		public override GeoPoint Location
		{
			get { return this.Model == null ? GeoPoint.Empty : this.Model.Location; }
			set	{ }
		}

		protected override object GetIcon()
		{
			return _resourceManager.GetRouteStopMarker(this.MarkerSize);
		}
	}
}