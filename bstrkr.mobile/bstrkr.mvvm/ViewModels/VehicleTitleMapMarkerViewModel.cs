using System;

using bstrkr.core.services.resources;
using bstrkr.core;

namespace bstrkr.mvvm.viewmodels
{
	public class VehicleTitleMapMarkerViewModel : MapMarkerViewModel
	{
		public VehicleTitleMapMarkerViewModel(IAppResourceManager resourceManager) : base(resourceManager)
		{
		}

		public VehicleTypes Type { get; set; }

		public string RouteNumber { get; set; }

		protected override object GetIcon(IAppResourceManager resourceManager)
		{
			return null;
//			return resourceManager.GetVehicleMarker(this.Type, this.Size, this.IsSelected);
		}

		public override string Key { get { return Consts.VehicleTitleMarkerKey; } }
	}
}