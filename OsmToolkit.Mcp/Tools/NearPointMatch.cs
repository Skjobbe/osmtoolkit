namespace OsmToolkit.Mcp.Tools
{
    /// <summary>
    /// A single <see cref="Node"/> matched by a near-point search, with just enough detail for an MCP tool caller.
    /// </summary>
    /// <param name="Id">The matched node's OSM id.</param>
    /// <param name="Tags">The matched node's tags.</param>
    /// <param name="Latitude">The matched node's latitude.</param>
    /// <param name="Longitude">The matched node's longitude.</param>
    /// <param name="DistanceMeters">The great-circle distance from the searched place's centroid to the matched node, in meters.</param>
    public sealed record NearPointMatch(long Id, IReadOnlyDictionary<string, string> Tags, double Latitude, double Longitude, double DistanceMeters);
}
