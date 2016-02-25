using System;

using bstrkr.core;
using bstrkr.core.map;
using bstrkr.core.services.resources;
using bstrkr.core.spatial;

using Cirrious.MvvmCross.ViewModels;

namespace bstrkr.mvvm.viewmodels
{
	public class RouteStopMapViewModel : MapMarkerViewModelBase<RouteStop>
	{
		public RouteStopMapViewModel(IAppResourceManager resourceManager) : base(resourceManager)
		{
		}

		public override GeoLocation Location
		{
			get { return this.Model == null ? GeoLocation.Empty : this.Model.Location; }
			set	{ }
		}

		protected override void SetIcons(IAppResourceManager resourceManager)
		{
			this.Icon = resourceManager.GetRouteStopMarker(this.MarkerSize, this.IsSelected);
		}
	}
}