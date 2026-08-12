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

            var nodes = dto.Elements?
                .Where(e => e.Type == "node")
                .Select(e => new Node(e.Id, e.Lat!.Value, e.Lon!.Value, e.Tags ?? new()))
                .ToList();

            var ways = dto.Elements?
                .Where(e => e.Type == "way")
                .Select(e => new Way(e.Id, e.Nodes ?? new(), e.Tags ?? new()))
                .ToList();

            var relations = dto.Elements?
                .Where(e => e.Type == "relation")
                .Select(e => new Relation(e.Id, e.Members!.Select(m => m.ToDomain()).ToList(), e.Tags ?? new()))
                .ToList();

            return new OsmData(header, null, nodes, ways, relations);
        }        
    }
}
