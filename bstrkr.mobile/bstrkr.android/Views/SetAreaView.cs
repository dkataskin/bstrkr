using System;
using System.Linq;

using Android.App;
using Android.OS;
using Android.Views;

using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Droid.Fragging.Fragments;

using bstrkr.mvvm.viewmodels;

namespace bstrkr.android.views
{
	public class SetAreaView : MvxDialogFragment
	{
		private int _selectedIndex;

		public override Dialog OnCreateDialog(Bundle savedState)
		{
			base.EnsureBindingContextSet(savedState);

			var view = this.BindingInflate(Resource.Layout.fragment_set_area_view, null);

			var viewModel = this.ViewModel as SetAreaViewModel;

			var dialog = new AlertDialog.Builder(Activity);
			dialog.SetTitle(Resources.GetString(Resource.String.about_view_title));
			dialog.SetView(view);
			dialog.SetPositiveButton("Select", (s, a) => viewModel.SelectArea.Execute(_selectedIndex));
			dialog.SetNegativeButton("Cancel", (s, a) => { });
			dialog.SetSingleChoiceItems(
							viewModel.Areas.Select(x => x.Name).ToArray(), 
							0, 
							(s, a) => _selectedIndex = a.Which);

			return dialog.Create();
		}
	}
}