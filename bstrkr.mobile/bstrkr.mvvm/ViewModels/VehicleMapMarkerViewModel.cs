using System;

using bstrkr.core.services.resources;
using bstrkr.core;

namespace bstrkr.mvvm.viewmodels
{
	public class VehicleMapMarkerViewModel : MapMarkerViewModel
	{
		public VehicleMapMarkerViewModel(IAppResourceManager resourceManager) : base(resourceManager)
		{
		}

		public VehicleTypes Type { get; set; }

		protected override object GetIcon(IAppResourceManager resourceManager)
		{
			return resourceManager.GetVehicleMarker(this.Type, this.Size, this.IsSelected);
		}

		public override string Key { get { return Consts.VehicleMarkerKey; } }
	}
}