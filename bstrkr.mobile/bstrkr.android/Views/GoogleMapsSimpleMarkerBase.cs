using System;

using System.Collections.Generic;
using System.Collections.ObjectModel;

using Android.Gms.Maps.Model;

using bstrkr.core.android.extensions;
using bstrkr.core.spatial;
using bstrkr.mvvm.viewmodels;
using bstrkr.mvvm.views;

namespace bstrkr.android.views
{
	public abstract class GoogleMapsSimpleMarkerBase : GoogleMapsMarkerBase
	{
		private Marker _marker;

		public Marker Marker { get { return _marker; } }

		public abstract MarkerOptions GetSingleMarkerOptions();

		public override IDictionary<string, MarkerOptions> GetOptions()
		{
			return new Dictionary<string, MarkerOptions> 
			{ 
				{ "main", this.GetSingleMarkerOptions() }
			};
		}
	}
}