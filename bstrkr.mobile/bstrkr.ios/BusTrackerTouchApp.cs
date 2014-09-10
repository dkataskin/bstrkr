using System;

using Cirrious.CrossCore;
using bstrkr.mvvm;
using bstrkr.core.services.location;
using bstrkr.core.ios.service.location;

namespace bstrkr.ios
{
	public class BusTrackerTouchApp : BusTrackerApp
	{
		public override void Initialize()
		{
			base.Initialize();

			Mvx.LazyConstructAndRegisterSingleton<ILocationService, LocationService>();
		}
	}
}