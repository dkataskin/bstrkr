using System;
using System.Collections.Generic;

using bstrkr.core;
using bstrkr.core.map;
using bstrkr.core.services.resources;
using bstrkr.core.spatial;

using Cirrious.MvvmCross.ViewModels;
using Cirrious.CrossCore;

namespace bstrkr.mvvm.viewmodels
{
	public class RouteStopMapViewModel : MapMarkerCompositeViewModelBase<RouteStop>
	{
		public override GeoLocation Location
		{
			get { return this.Model == null ? GeoLocation.Empty : this.Model.Location; }
			set	{ }
		}

		protected override IEnumerable<MapMarkerViewModel> GetMapMarkerViewModels(RouteStop model)
		{
			return new[] { Mvx.Resolve<RouteStopMapMarkerViewModel>() };
		}
	}
}