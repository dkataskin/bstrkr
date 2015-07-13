using System;

using Android.OS;
using Android.Runtime;
using Android.Views;

using bstrkr.core.android.views;

using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Droid.FullFragging.Fragments;

namespace bstrkr.android.views
{
	[Register("bstrkr.android.views.InitView")]
	public class InitView : MvxFragment
	{
		public InitView()
		{
			this.RetainInstance = false;
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			var ignored = base.OnCreateView(inflater, container, savedInstanceState);

			//(this.Activity as MvxActionBarActivity).SupportActionBar.Hide();

			return this.BindingInflate(Resource.Layout.fragment_init_view, null);
		}
	}
}