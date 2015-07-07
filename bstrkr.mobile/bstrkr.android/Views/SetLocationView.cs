using System;

using Android.App;
using Android.OS;
using Android.Runtime;

using bstrkr.mvvm.viewmodels;

using Cirrious.MvvmCross.Droid.FullFragging.Fragments;

namespace bstrkr.android.views
{
	[Register("bstrkr.android.views.SetLocationView")]
	public class SetLocationView : MvxDialogFragment
	{
		public override Dialog OnCreateDialog(Bundle savedState)
		{
			base.EnsureBindingContextSet(savedState);

			var viewModel = this.ViewModel as LocationViewModel;

			var dialog = new Android.Support.V7.App.AlertDialog.Builder(Activity);

			return dialog.Create();
		}
	}
}