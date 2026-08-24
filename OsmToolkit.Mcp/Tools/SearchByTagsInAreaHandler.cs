using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OsmToolkit.DataSources;
using OsmToolkit.Finders;
using OsmToolkit.Geocoding;
using OsmToolkit.Mcp.Tools.Logging;

namespace OsmToolkit.Mcp.Tools
{
    /// <summary>
    /// Application logic behind the <c>search_by_tags_in_area</c> MCP tool: resolves a place name to an area,
    /// fetches OSM data for it, and filters it down to entities matching the requested tags. Depends only on
    /// already-registered library interfaces, so it can be constructed and called directly in a test, without
    /// any MCP-specific transport or attribute involved.
    /// </summary>
    public class SearchByTagsInAreaHandler
    {
        private readonly IPlaceLookup _placeLookup;
        private readonly IOsmDataSource _dataSource;
        private readonly IOsmValueFinder<OsmEntity> _valueFinder;
        private readonly ILogger<SearchByTagsInAreaHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchByTagsInAreaHandler"/> class.
        /// </summary>
        /// <param name="placeLookup">Resolves the free-text place name to a geographic area.</param>
        /// <param name="dataSource">Fetches OSM data for the resolved area.</param>
        /// <param name="valueFinder">Filters the fetched data down to entities matching the requested tags.</param>
        /// <param name="logger">An optional logger for diagnostics. If not provided, a <see cref="NullLogger{SearchByTagsInAreaHandler}"/> is used.</param>
        public SearchByTagsInAreaHandler(
            IPlaceLookup placeLookup,
            IOsmDataSource dataSource,
            IOsmValueFinder<OsmEntity> valueFinder,
            ILogger<SearchByTagsInAreaHandler>? logger = null)
        {
            _placeLookup = placeLookup;
            _dataSource = dataSource;
            _valueFinder = valueFinder;
            _logger = logger ?? new NullLogger<SearchByTagsInAreaHandler>();
        }

        /// <summary>
        /// Searches for <see cref="OsmEntity"/> instances matching <paramref name="tags"/> within <paramref name="place"/>.
        /// </summary>
        /// <param name="place">A free-text place name, e.g. a city, address, or landmark.</param>
        /// <param name="tags">
        /// Tag filters to match, keyed by OSM tag key. A <c>null</c> value matches any value for that key,
        /// mirroring <see cref="IOsmValueFinder{T}.FindByTag(OsmData, string, string?)"/>'s existing overloads.
        /// </param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>The matching entities, with their tags and resolved coordinates.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="place"/> is null or empty, or <paramref name="tags"/> is null or empty.</exception>
        /// <exception cref="PlaceNotFoundException">Thrown when no place matches <paramref name="place"/>.</exception>
        public async Task<IReadOnlyList<TagSearchMatch>> SearchAsync(string place, IReadOnlyDictionary<string, string?> tags, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(place))
                throw new ArgumentException("Place cannot be null or empty.", nameof(place));

            if (tags == null || tags.Count == 0)
                throw new ArgumentException("At least one tag filter must be specified.", nameof(tags));

            SearchByTagsInAreaLogMessages.LogSearchStart(_logger, place, tags.Count);

            var location = await _placeLookup.FindAsync(place, cancellationToken);
            var data = await _dataSource.GetOsmDataAsync(location.Bounds, cancellationToken);

            var tagFilter = tags.ToDictionary(kv => kv.Key, kv => kv.Value ?? string.Empty);
            var filtered = _valueFinder.FindByTags(data, tagFilter);

            var nodesById = data.Nodes.ToDictionary(n => n.Id);
            var waysById = data.Ways.ToDictionary(w => w.Id);

            var matches = new List<TagSearchMatch>(filtered.Nodes.Count + filtered.Ways.Count + filtered.Relations.Count);
            matches.AddRange(filtered.Nodes.Select(node => ToMatch(node, (node.Latitude, node.Longitude))));
            matches.AddRange(filtered.Ways.Select(way => ToMatch(way, ResolveWayCoordinates(way, nodesById))));
            matches.AddRange(filtered.Relations.Select(relation => ToMatch(relation, ResolveRelationCoordinates(relation, nodesById, waysById))));

            SearchByTagsInAreaLogMessages.LogSearchResult(_logger, place, matches.Count);

            return matches;
        }

        private static TagSearchMatch ToMatch(OsmEntity entity, (double? Latitude, double? Longitude) coordinates) =>
            new(entity.Id, EntityTypeOf(entity), entity.Tags, coordinates.Latitude, coordinates.Longitude);

        private static string EntityTypeOf(OsmEntity entity) => entity switch
        {
            Node => nameof(ReferenceType.node),
            Way => nameof(ReferenceType.way),
            Relation => nameof(ReferenceType.relation),
            _ => entity.GetType().Name,
        };

        private static (double? Latitude, double? Longitude) ResolveWayCoordinates(Way way, IReadOnlyDictionary<long, Node> nodesById) =>
            AverageCoordinates(way.NodeReferenceIds
                .Select(id => nodesById.TryGetValue(id, out var node) ? node : null)
                .Where(node => node is not null)
                .Select(node => (node!.Latitude, node.Longitude)));

        /// <summary>
        /// Resolves a relation's centroid from its directly-referenced node and way members. Sub-relation members
        /// are not followed, keeping resolution to a single level of nesting rather than recursing into the
        /// (possibly cyclic) relation graph.
        /// </summary>
        private static (double? Latitude, double? Longitude) ResolveRelationCoordinates(Relation relation, IReadOnlyDictionary<long, Node> nodesById, IReadOnlyDictionary<long, Way> waysById)
        {
            var points = new List<(double Latitude, double Longitude)>();

            foreach (var member in relation.Members)
            {
                switch (member.Type)
                {
                    case ReferenceType.node:
                        if (nodesById.TryGetValue(member.ReferenceId, out var node))
                            points.Add((node.Latitude, node.Longitude));
                        break;

                    case ReferenceType.way:
                        if (waysById.TryGetValue(member.ReferenceId, out var way))
                        {
                            points.AddRange(way.NodeReferenceIds
                                .Select(id => nodesById.TryGetValue(id, out var wayNode) ? wayNode : null)
                                .Where(wayNode => wayNode is not null)
                                .Select(wayNode => (wayNode!.Latitude, wayNode.Longitude)));
                        }
                        break;
                }
            }

            return AverageCoordinates(points);
        }

        private static (double? Latitude, double? Longitude) AverageCoordinates(IEnumerable<(double Latitude, double Longitude)> points)
        {
            var list = points as IReadOnlyCollection<(double Latitude, double Longitude)> ?? points.ToList();
            return list.Count == 0
                ? (null, null)
                : (list.Average(p => p.Latitude), list.Average(p => p.Longitude));
        }
    }
}
