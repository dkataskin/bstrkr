using System;

using Android;
using Android.Gms.Maps.Model;

using Cirrious.CrossCore.Platform;

using bstrkr.core.services.resources;

namespace bstrkr.core.android.services.resources
{
	public class ResourceManager : ResourceManagerBase
	{
		protected override object GetImageResource(string path)
		{
			return BitmapDescriptorFactory.FromPath("drawable/" + path);
		}
	}
}