using System;

using Android.App;
using Android.OS;

using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Droid.Fragging.Fragments;

namespace bstrkr.android.views
{
	public class AboutView : MvxDialogFragment
	{
		public override Dialog OnCreateDialog(Bundle savedState)
		{
			base.EnsureBindingContextSet(savedState);

			var view = this.BindingInflate(Resource.Layout.fragment_about_view, null);

			var dialog = new AlertDialog.Builder(Activity);
			dialog.SetTitle(Resources.GetString(Resource.String.about_view_title));
			dialog.SetView(view);
			dialog.SetNegativeButton("Cancel", (s, a) => { });
			dialog.SetPositiveButton("OK", (s, a) => { });

			return dialog.Create();
		}
	}
}