using System;
using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.OS;
using Android.Runtime;

using bstrkr.core;
using bstrkr.core.config;
using bstrkr.mvvm.viewmodels;

using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Droid.FullFragging.Fragments;

namespace bstrkr.android.views
{
	[Register("bstrkr.android.views.SetAreaView")]
	public class SetAreaView : MvxDialogFragment
	{
		private int _selectedIndex;

		public override Dialog OnCreateDialog(Bundle savedState)
		{
			base.EnsureBindingContextSet(savedState);

			var viewModel = this.ViewModel as SetAreaViewModel;

			var dialog = new Android.Support.V7.App.AlertDialog.Builder(Activity);
			dialog.SetTitle(AppResources.locations_dialog_title);
			dialog.SetPositiveButton(AppResources.select, (s, a) => viewModel.SelectAreaCommand.Execute(_selectedIndex));
			dialog.SetNegativeButton(AppResources.cancel, (s, a) => viewModel.CancelCommand.Execute());
			dialog.SetSingleChoiceItems(
							viewModel.Areas.Select(x => x.Name).ToArray(), 
							0, 
							(s, a) => _selectedIndex = a.Which);

			return dialog.Create();
		}
	}
}