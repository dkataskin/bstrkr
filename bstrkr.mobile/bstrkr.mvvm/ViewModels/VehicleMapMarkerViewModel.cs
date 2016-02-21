using System;

namespace bstrkr.mvvm.viewmodels
{
	public class VehicleMapMarkerViewModel : MapMarkerViewModel
	{
		public VehicleMapMarkerViewModel()
		{
		}

		#region implemented abstract members of MapMarkerViewModel

		protected override object GetIcon()
		{
			throw new NotImplementedException();
		}

		public override string Key {
			get {
				throw new NotImplementedException();
			}
		}

		#endregion
	}
}

