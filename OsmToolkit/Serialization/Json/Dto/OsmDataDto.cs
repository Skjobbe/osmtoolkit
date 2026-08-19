using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OsmToolkit.Serialization.Json.Dto
{
    internal class OsmDataDto
    {
        public HeaderDto Header { get; set; } = new();
        public OsmCoordinateBoundsDto Bounds { get; set; } = new();
        public List<NodeDto> Nodes { get; set; } = new();
        public List<WayDto> Ways { get; set; } = new();
        public List<RelationDto> Relations { get; set; } = new();

        public OsmData FromJson()
        {
            var header = new OsmHeader(
                Header.Version,
                Header.Generator,
                Header.Copyright,
                Header.Attribution,
                Header.License);

            var bounds = new OsmCoordinateBounds(
                Bounds.Minlat,
                Bounds.Minlon,
                Bounds.Maxlat,
                Bounds.Maxlon);

            var nodes = Nodes.Select(n =>  n.ToDomain());
            var ways = Ways.Select(w =>  w.ToDomain());
            var relations = Relations.Select(r =>  r.ToDomain());

            return new OsmData(header, bounds, nodes, ways, relations);
        }

        public static OsmData FromOverpassJson(OverpassJsonDto dto)
        {
            var header = new OsmHeader(
                dto.Version,
                dto.Generator,
                null,
                null,
                null);

            var nodes = new List<Node>();
            var ways = new List<Way>();
            var relations = new List<Relation>();

            foreach (var element in dto.Elements ?? new())
            {
                switch (element.ToDomain())
                {
                    case Node node:
                        nodes.Add(node);
                        break;
                    case Way way:
                        ways.Add(way);
                        break;
                    case Relation relation:
                        relations.Add(relation);
                        break;
                }
            }

            return new OsmData(header, null, nodes, ways, relations);
        }

    }
}
