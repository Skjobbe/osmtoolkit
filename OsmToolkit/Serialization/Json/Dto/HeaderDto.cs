using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsmToolkit.Serialization.Json.Dto
{
    internal class HeaderDto
    {
        public double Version { get; set; }
        public string? Generator { get; set; } = string.Empty;
        public string? Copyright { get; set; } = string.Empty;
        public string? Attribution { get; set; } = string.Empty;
        public string? License { get; set; } = string.Empty;       
    }
}
