using System.Text.Json.Serialization;
namespace OsmToolkit
{
    /// <summary>
    /// Contains lists including all <see cref="Node"/>, <see cref="Way"/> and <see cref="Relation"/> instances registered from OSM-sources.
    /// </summary>
    public class OsmData
    {
        /// <summary>
        /// <see cref="OsmHeader"/> describing header details for the OSM-data.
        /// </summary>
        [JsonPropertyName("header"), JsonPropertyOrder(0)]
        public OsmHeader Header { get; set;  }
        /// <summary>
        /// <see cref="OsmCoordinateBounds"/> set for the OSM-data.
        /// </summary>
        [JsonPropertyName("bounds"), JsonPropertyOrder(1)]
        public OsmCoordinateBounds? Bounds { get; } = null;
        /// <summary>
        /// List of registered <see cref="Node"/> instances.
        /// </summary>
        [JsonPropertyName("nodes"), JsonPropertyOrder(2)]
        public IList<Node> Nodes { get; private set; } = new List<Node>();
        /// <summary>
        /// List of registered <see cref="Way"/> instances.
        /// </summary>
        [JsonPropertyName("ways"), JsonPropertyOrder(3)]
        public IList<Way> Ways { get; private set; } = new List<Way>();
        /// <summary>
        /// List of registered <see cref="Relation"/> instances.
        /// </summary>
        [JsonPropertyName("relations"), JsonPropertyOrder(4)]
        public IList<Relation> Relations { get; private set; } = new List<Relation>();

        /// <summary>
        /// Initializes a new <see cref="OsmData"/> object without any registered instances of <see cref="OsmEntity"/>.
        /// </summary>
        /// <param name="header">The <see cref="OsmHeader"/> describing the header details for the OSM-data.</param>
        /// <exception cref="ArgumentNullException">Throws if <paramref name="header"/> is <c>null</c>.</exception>
        public OsmData(OsmHeader header) 
        {
            if (header is null)
                throw new ArgumentNullException(nameof(header), $"Header cannot be null, must be defined for OSM-data.");

            Header = header;
        }

        /// <summary>
        /// Initializes a new <see cref="OsmData"/> object with registered instances of class types <see cref="Node"/>, <see cref="Way"/> and <see cref="Relation"/>.
        /// </summary>
        /// <param name="header">The <see cref="OsmHeader"/> describing the header details for the OSM-data.</param>
        /// <param name="bounds">The <see cref="OsmCoordinateBounds"/> set for the OSM-data, can be <c>null</c>.</param>
        /// <param name="nodes">List of <see cref="Node"/> instances, can be <c>null</c>.</param>
        /// <param name="ways">List of <see cref="Way"/> instances, can be <c>null</c>.</param>
        /// <param name="relations">List of <see cref="Relation"/> instances, can be <c>null</c>.</param>
        /// <exception cref="ArgumentNullException">Throws if <paramref name="header"/> is <c>null</c>.</exception>
        public OsmData(OsmHeader header, OsmCoordinateBounds? bounds = null, IEnumerable<Node>? nodes = null, IEnumerable<Way>? ways = null, IEnumerable<Relation>? relations = null)
            : this(header)
        {
            Bounds = bounds;

            var seenNodeIds = new HashSet<long>();
            Nodes = nodes?
                .Where(n => seenNodeIds.Add(n.Id))
                .ToList() ?? new List<Node>();

            var seenWayIds = new HashSet<long>();
            Ways = ways?
                .Where(w => seenWayIds.Add(w.Id))
                .ToList() ?? new List<Way>();

            var seenRelationIds = new HashSet<long>();
            Relations = relations?
                .Where(r => seenRelationIds.Add(r.Id))
                .ToList() ?? new List<Relation>();

            //Nodes = nodes?.ToList() ?? new List<Node>();
            //Ways = ways?.ToList() ?? new List<Way>();
            //Relations = relations?.ToList() ?? new List<Relation>();
        }
        /// <summary>
        /// Sorts all OSM entities by their unique identifiers (Id) to ensure consistent serialization order.
        /// </summary>
        public void SortOsmData()
        {
            if (Nodes.Count > 1)
            {
                Nodes = Nodes.OrderBy(n => n.Id).ToList();
            }
            if (Ways.Count > 1)
            {
                Ways = Ways.OrderBy(w => w.Id).ToList();
            }
            if (Relations.Count > 1)
            {
                Relations = Relations.OrderBy(r => r.Id).ToList();
            }
        }


    }
}
