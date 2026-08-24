namespace OsmToolkit.Mcp.Tools
{
    /// <summary>
    /// A single <see cref="OsmEntity"/> matched by a tag search, with just enough detail for an MCP tool caller.
    /// </summary>
    /// <param name="Id">The matched entity's OSM id.</param>
    /// <param name="EntityType">The matched entity's OSM type: <c>"node"</c>, <c>"way"</c>, or <c>"relation"</c>.</param>
    /// <param name="Tags">The matched entity's tags.</param>
    /// <param name="Latitude">
    /// The matched entity's latitude. Exact for a <see cref="Node"/>; the average of its referenced nodes'
    /// coordinates for a <see cref="Way"/> or <see cref="Relation"/>, or <c>null</c> if none of those nodes
    /// were present in the fetched data.
    /// </param>
    /// <param name="Longitude">The matched entity's longitude, resolved the same way as <see cref="Latitude"/>.</param>
    public sealed record TagSearchMatch(long Id, string EntityType, IReadOnlyDictionary<string, string> Tags, double? Latitude, double? Longitude);
}
