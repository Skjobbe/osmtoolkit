using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsmToolkit.Serialization.Json.Dto
{
    internal class OsmCoordinateBoundsDto
    {
        public double Minlat { get; set; }
        public double Minlon { get; set; }
        public double Maxlat { get; set; }
        public double Maxlon { get; set; }
    }
}
