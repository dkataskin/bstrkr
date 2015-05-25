using System;
using System.Linq;

using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Views;

using bstrkr.core;
using bstrkr.mvvm.viewmodels;

using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Droid.FullFragging.Fragments;

namespace bstrkr.android.views
{
	[Register("bstrkr.android.views.SetRouteStopView")]
	public class SetRouteStopView : MvxDialogFragment
	{
		private int _selectedIndex;

		public override Dialog OnCreateDialog(Bundle savedState)
		{
			base.EnsureBindingContextSet(savedState);

			var viewModel = this.ViewModel as SetRouteStopViewModel;

			var dialog = new AlertDialog.Builder(Activity);
			dialog.SetTitle(AppResources.stops_dialog_title);
			dialog.SetPositiveButton(AppResources.select, (s, a) => viewModel.SelectRouteStopCommand.Execute(_selectedIndex));
			dialog.SetNegativeButton(AppResources.cancel, (s, a) => { });
			dialog.SetSingleChoiceItems(
							viewModel.RouteStops.ToArray(), 
							0, 
							(s, a) => _selectedIndex = a.Which);

			return dialog.Create();
		}
	}
}