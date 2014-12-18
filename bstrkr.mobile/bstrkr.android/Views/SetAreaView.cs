using System;
using System.Linq;

using Android.App;
using Android.OS;
using Android.Views;

using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Droid.Fragging.Fragments;

using bstrkr.mvvm.viewmodels;
using bstrkr.core;

namespace bstrkr.android.views
{
	public class SetAreaView : MvxDialogFragment
	{
		private int _selectedIndex;

		public override Dialog OnCreateDialog(Bundle savedState)
		{
			base.EnsureBindingContextSet(savedState);

			var viewModel = this.ViewModel as SetAreaViewModel;

			var dialog = new AlertDialog.Builder(Activity);
			dialog.SetTitle(AppResources.locations_dialog_title);
			dialog.SetPositiveButton(AppResources.select, (s, a) => viewModel.SelectArea.Execute(_selectedIndex));
			dialog.SetNegativeButton(AppResources.cancel, (s, a) => { });
			dialog.SetSingleChoiceItems(
							viewModel.Areas.Select(x => x.Name).ToArray(), 
							0, 
							(s, a) => _selectedIndex = a.Which);

			return dialog.Create();
		}
	}
}