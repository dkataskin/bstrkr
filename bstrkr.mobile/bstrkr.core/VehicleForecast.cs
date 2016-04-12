using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace bstrkr.core
{
    public class VehicleForecast
    {
        public VehicleForecast(Vehicle vehicle, IEnumerable<VehicleForecastItem> items)
        {
            this.Vehicle = vehicle;
            this.Items = new ReadOnlyCollection<VehicleForecastItem>(items.ToList());
        }

        public Vehicle Vehicle { get; private set; }

        public IReadOnlyList<VehicleForecastItem> Items { get; private set; }
    }
}