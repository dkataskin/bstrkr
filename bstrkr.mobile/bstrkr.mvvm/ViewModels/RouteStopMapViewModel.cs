using bstrkr.core;
using bstrkr.core.services.resources;
using bstrkr.core.spatial;

namespace bstrkr.mvvm.viewmodels
{
    public class RouteStopMapViewModel : MapMarkerViewModelBase<RouteStop>
    {
        public RouteStopMapViewModel(IAppResourceManager resourceManager) : base(resourceManager)
        {
        }

        public override GeoLocation Location
        {
            get { return this.Model?.Location ?? GeoLocation.Empty; }
            set { }
        }

        protected override void SetIcons(IAppResourceManager resourceManager)
        {
            this.Icon = resourceManager.GetRouteStopMarker(this.MarkerSize, this.IsSelected);
        }
    }
}