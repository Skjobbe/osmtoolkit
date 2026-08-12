using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsmToolkit.Serialization.Json.Dto
{
    internal class OsmJsonElementDto
    {
        public string Type { get; set; } = string.Empty;
        public long Id { get; set; }
        public double? Lat { get; set; }
        public double? Lon { get; set; }
        public List<long>? Nodes { get; set; }
        public List<MemberDto>? Members { get; set; }
        public Dictionary<string, string>? Tags { get; set; }
    }
}
