using System;

using Android;
using Android.Gms.Maps.Model;

using Cirrious.CrossCore.Platform;

using bstrkr.core.services.resources;
using System.IO;

namespace bstrkr.core.android.services.resources
{
	public class ResourceManager : ResourceManagerBase
	{
		protected override object GetImageResource(string path)
		{
			var context = Android.App.Application.Context;

			var id = context.Resources.GetIdentifier(
							Path.GetFileNameWithoutExtension(path), 
							"drawable", 
							context.PackageName);

			return BitmapDescriptorFactory.FromResource(id);
		}
	}
}