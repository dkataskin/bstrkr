using System;

using Android.OS;
using Android.Views;

using bstrkr.core;
using bstrkr.mvvm.viewmodels;

using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Droid.Fragging.Fragments;
using bstrkr.mvvm.converters;

namespace bstrkr.android.views
{
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
		
			this.Activity.ActionBar.Title = routeNumberConverter.Convert(dataContext.Number.ToString(), dataContext.VehicleType);

			return this.BindingInflate(Resource.Layout.fragment_route_view, null);
		}
	}
}