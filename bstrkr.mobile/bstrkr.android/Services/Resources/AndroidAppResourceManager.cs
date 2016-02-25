using System;
using System.IO;

using Android;
using Android.Gms.Maps.Model;

using bstrkr.android.util;
using bstrkr.core;
using bstrkr.core.services.resources;

using Cirrious.CrossCore.Platform;
using Cirrious.CrossCore.Droid;

namespace bstrkr.android.services.resources
{
	public class AndroidAppResourceManager : AppResourceManagerBase
	{
//		private readonly IMvxAndroidGlobals _androidGlobals;
//		private readonly IconGenerator _iconGenerator;

		public AndroidAppResourceManager(/*IMvxAndroidGlobals androidGlobals*/)
		{
//			_androidGlobals = androidGlobals;
//			_iconGenerator = new IconGenerator(_androidGlobals.ApplicationContext);
		}

		public IconGenerator IconGenerator { get; set; }

		protected override object GetImageResource(string path)
		{
			var context = Android.App.Application.Context;

			var id = context.Resources.GetIdentifier(
							Path.GetFileNameWithoutExtension(path), 
							"drawable", 
							context.PackageName);

			return BitmapDescriptorFactory.FromResource(id);
		}

		public override object GetVehicleTitleMarker(VehicleTypes type, string title)
		{
			return this.IconGenerator.MakeIcon(title);
		}
	}
}