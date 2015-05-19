﻿using System;

using Android.OS;
using Android.Views;

using bstrkr.core;
using bstrkr.mvvm;
using bstrkr.mvvm.viewmodels;

using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Droid.FullFragging.Fragments;

namespace bstrkr.android.views
{
	public class RouteStopView : MvxFragment
	{
		public RouteStopView()
		{
			this.RetainInstance = true;
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			var ignored = base.OnCreateView(inflater, container, savedInstanceState);

			this.SetHasOptionsMenu(true);

			var vm = this.DataContext as RouteStopViewModel;
			this.Activity.ActionBar.Title = string.Format(
													AppResources.route_stop_view_title_format,
													vm.Name,
													vm.Description);

			return this.BindingInflate(Resource.Layout.fragment_routestop_view, null);
		}

		public override void OnDestroyView()
		{
			base.OnDestroyView();

			if (this.DataContext != null && this.DataContext is ICleanable)
			{
				(this.DataContext as ICleanable).CleanUp();
			}
		}
	}
}