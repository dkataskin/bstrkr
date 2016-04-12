using Android.OS;
using Android.Runtime;
using Android.Views;

using bstrkr.core.android.views;
using bstrkr.mvvm;
using bstrkr.mvvm.converters;
using bstrkr.mvvm.viewmodels;

using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Droid.FullFragging.Fragments;

namespace bstrkr.android.views
{
    [Register("bstrkr.android.views.RouteView")]
    public class RouteView : MvxFragment
    {
        public RouteView()
        {
            this.RetainInstance = true;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            this.SetHasOptionsMenu(true);

            var ignored = base.OnCreateView(inflater, container, savedInstanceState);

            var routeNumberConverter = new RouteInfoToTitleConverter();
            var dataContext = this.DataContext as RouteVehiclesViewModel;

            (this.Activity as MvxAppCompatActivity).SupportActionBar.Title =
                routeNumberConverter.Convert(dataContext.Number.ToString(), dataContext.VehicleType);

            return this.BindingInflate(Resource.Layout.fragment_route_view, null);
        }

        public override void OnPrepareOptionsMenu(IMenu menu)
        {
            menu.Clear();
            base.OnPrepareOptionsMenu(menu);
        }

        public override void OnDestroyView()
        {
            base.OnDestroyView();

            (this.DataContext as ICleanable)?.CleanUp();
        }
    }
}