using System.Collections.Generic;
using System.Collections.Immutable;

namespace bstrkr.mvvm.viewmodels
{
    public class VisibleVehiclesDelta
    {
        public VisibleVehiclesDelta(IEnumerable<VehicleViewModel> toAdd, IEnumerable<VehicleViewModel> toRemove)
        {
            this.VehiclesToAdd = toAdd.ToImmutableList();
            this.VehiclesToRemove = toRemove.ToImmutableList();
        }

        public static VisibleVehiclesDelta ClearVisibleVehiclesDelta => new VisibleVehiclesDelta(
                                                                            ImmutableList<VehicleViewModel>.Empty, 
                                                                            ImmutableList<VehicleViewModel>.Empty)
                                                                        {
                                                                            Clear = true
                                                                        };

        public ImmutableList<VehicleViewModel> VehiclesToRemove { get; }
        
        public ImmutableList<VehicleViewModel> VehiclesToAdd { get; }

        public bool Clear { get; private set; }
    }
}