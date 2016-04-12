using Android.OS;
using Android.Runtime;
using Android.Views;

using bstrkr.core;
using bstrkr.core.android.views;

using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Droid.FullFragging.Fragments;

namespace bstrkr.android.views
{
    [Register("bstrkr.android.views.AboutView")]
    public class AboutView : MvxFragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            this.SetHasOptionsMenu(true);

            var ignored = base.OnCreateView(inflater, container, savedInstanceState);

            (this.Activity as MvxAppCompatActivity).SupportActionBar.Title = AppResources.about_view_title;

            return this.BindingInflate(Resource.Layout.fragment_about_view, null);
        }

        public override void OnPrepareOptionsMenu(IMenu menu)
        {
            menu.Clear();
            base.OnPrepareOptionsMenu(menu);
        }
    }
}