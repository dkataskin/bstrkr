using System;

using bstrkr.core.services.resources;

namespace bstrkr.mvvm.viewmodels
{
	public class RouteStopMapMarkerViewModel : MapMarkerViewModel
	{
		public RouteStopMapMarkerViewModel(IAppResourceManager resourceManager) : base(resourceManager)
		{
		}

		protected override object GetIcon(IAppResourceManager resourceManager)
		{
			return resourceManager.GetRouteStopMarker(this.Size, this.IsSelected);
		}

		public override string Key { get { return Consts.RouteStopMarkerKey; } }
	}
}