﻿using System;
using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V13.App;
using Android.Support.V4.View;
using Android.Views;
using Android.Widget;

using bstrkr.core;
using bstrkr.mvvm;
using bstrkr.mvvm.viewmodels;

using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Droid.Fragging.Fragments;

using DK.Ostebaronen.Droid.ViewPagerIndicator;

namespace bstrkr.android.views
{
	public class MapRouteStopView : MvxFragment
	{
		public MapRouteStopView()
		{
			this.RetainInstance = false;
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			var ignored = base.OnCreateView(inflater, container, savedInstanceState);

			return this.BindingInflate(Resource.Layout.fragment_map_routestop_view, null);
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