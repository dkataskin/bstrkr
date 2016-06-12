using System;

namespace bstrkr.mvvm.viewmodels
{
    public class VehicleSelectedEventArgs : EventArgs
    {
        public VehicleSelectedEventArgs(VehicleViewModel vehicle)
        {
            this.Vehicle = vehicle;
        }

        public VehicleViewModel Vehicle { get; }
    }
}