using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsmToolkit.Serialization.Json.Dto
{
    internal class OverpassJsonDto
    {
        public double Version { get; set; }
        public string? Generator { get; set; }
        public List<OsmJsonElementDto> Elements { get; set; } = new();
    }
}
