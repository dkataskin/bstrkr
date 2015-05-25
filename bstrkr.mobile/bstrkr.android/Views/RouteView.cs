using System;

using Android.OS;
using Android.Runtime;
using Android.Views;

using bstrkr.core;
using bstrkr.mvvm;
using bstrkr.mvvm.converters;
using bstrkr.mvvm.viewmodels;

using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Droid.FullFragging.Fragments;
using bstrkr.core.android;

namespace bstrkr.android.views
{
	[Register("bstrkr.android.views.RouteView")]
	public class RouteView : MvxFragment
	{
		public RouteView()
		{
			this.RetainInstance = true;
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			var ignored = base.OnCreateView(inflater, container, savedInstanceState);

			var routeNumberConverter = new RouteNumberToTitleConverter();
			var dataContext = this.DataContext as RouteViewModel;
		
			(this.Activity as MvxActionBarActivity).SupportActionBar.Title = 
				routeNumberConverter.Convert(dataContext.Number.ToString(), dataContext.VehicleType);

			return this.BindingInflate(Resource.Layout.fragment_route_view, null);
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