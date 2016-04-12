using System.Collections.Generic;

namespace bstrkr.providers.bus13.data
{
    public class Bus13VehicleLocationResponse
    {
        public int MaxK { get; set; }

        public List<Bus13VehicleLocation> Anims { get; set; }
    }
}