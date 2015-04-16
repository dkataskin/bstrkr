using System;

using Android.OS;
using Android.Views;

using bstrkr.mvvm;

using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Droid.Fragging.Fragments;

namespace bstrkr.android.views
{
	public class RouteVehicleForecastView : MvxFragment
	{
		public RouteVehicleForecastView()
		{
			this.RetainInstance = false;
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			var ignored = base.OnCreateView(inflater, container, savedInstanceState);

			return this.BindingInflate(Resource.Layout.fragment_vehicle_forecast_map_view, null);
		}

		public override void OnDestroyView()
		{
			base.OnDestroyView();
			if (this.DataContext is ICleanable)
			{
				(this.DataContext as ICleanable).CleanUp();
			}
		}
	}
}