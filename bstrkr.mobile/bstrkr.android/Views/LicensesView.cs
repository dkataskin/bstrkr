using Android.OS;
using Android.Runtime;
using Android.Views;

using bstrkr.core;
using bstrkr.core.android.views;

using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Droid.FullFragging.Fragments;

namespace bstrkr.android.views
{
    [Register("bstrkr.android.views.LicensesView")]
    public class LicensesView : MvxFragment
    {
        public LicensesView()
        {
            this.RetainInstance = true;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            this.SetHasOptionsMenu(true);

            var ignored = base.OnCreateView(inflater, container, savedInstanceState);

            (this.Activity as MvxAppCompatActivity).SupportActionBar.Title = AppResources.licenses_view_title;

            return this.BindingInflate(Resource.Layout.fragment_licenses_view, null);
        }

        public override void OnPrepareOptionsMenu(IMenu menu)
        {
            menu.Clear();
            base.OnPrepareOptionsMenu(menu);
        }
    }
}