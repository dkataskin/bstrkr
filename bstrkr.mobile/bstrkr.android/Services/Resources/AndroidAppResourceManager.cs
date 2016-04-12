using System.IO;

using Android.Gms.Maps.Model;

using bstrkr.android.util;
using bstrkr.core;
using bstrkr.core.services.resources;

namespace bstrkr.android.services.resources
{
    public class AndroidAppResourceManager : AppResourceManagerBase
    {
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