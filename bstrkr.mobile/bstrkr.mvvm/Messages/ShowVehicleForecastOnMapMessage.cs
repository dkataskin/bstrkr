using System;

using Cirrious.MvvmCross.Plugins.Messenger;

namespace bstrkr.mvvm.messages
{
	public class ShowVehicleForecastOnMapMessage : MvxMessage
	{
		public ShowVehicleForecastOnMapMessage(object sender, string vehicleId) : base(sender)
		{
			this.VehicleId = vehicleId;
		}

		public string VehicleId { get; private set; }
	}
}