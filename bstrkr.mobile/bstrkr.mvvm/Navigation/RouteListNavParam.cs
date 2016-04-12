using System.Collections.Generic;

namespace bstrkr.mvvm.navigation
{
    public class RouteListNavParam
    {
        public RouteListNavParam()
        {
            this.Routes = new List<RouteListItem>();
        }

        public List<RouteListItem> Routes { get; set; }
    }
}