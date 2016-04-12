using System;

using bstrkr.core;

namespace bstrkr.providers
{
    public class VehicleForecastReceivedEventArgs : EventArgs
    {
        public VehicleForecastReceivedEventArgs(string vehicleId, VehicleForecast forecast)
        {
            this.VehicleId = vehicleId;
            this.Forecast = forecast;
        }

        public string VehicleId { get; private set; }

        public VehicleForecast Forecast { get; private set; }
    }
}