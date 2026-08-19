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

        public OsmEntity ToDomain()
        {
            return Type switch
            {
                "node" => ToNode(),
                "way" => ToWay(),
                "relation" => ToRelation(),
                _ => throw new ArgumentOutOfRangeException(nameof(Type), Type, $"Overpass returned an item with unknown type '{Type}' (id {Id}).")
            };
        }

        private Node ToNode()
        {
            if (Lat is null)
            {
                throw new ArgumentNullException(nameof(Lat), $"Node {Id} is missing 'lat'.");
            }
            if (Lon is null)
            {
                throw new ArgumentNullException(nameof(Lon), $"Node {Id} is missing 'lon'.");
            }

            return new Node(Id, Lat.Value, Lon.Value, Tags ?? new());
        }

        private Way ToWay()
        {
            if (Nodes is null)
            {
                throw new ArgumentNullException(nameof(Nodes), $"Way {Id} is missing 'nodes'.");
            }
            if (Nodes.Count < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(Nodes), Nodes.Count, $"Way {Id} has {Nodes.Count} node reference(s); at least two are required.");
            }

            return new Way(Id, Nodes, Tags ?? new());
        }

        private Relation ToRelation()
        {
            if (Members is null)
            {
                throw new ArgumentNullException(nameof(Members), $"Relation {Id} is missing 'members'.");
            }
            if (Members.Count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(Members), Members.Count, $"Relation {Id} has no members; at least one is required.");
            }

            return new Relation(Id, Members.Select(m => m.ToDomain()).ToList(), Tags ?? new());
        }
    }
}
