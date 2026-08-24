using ModelContextProtocol.Server;
using System.ComponentModel;

namespace OsmToolkit.Mcp.Tools
{
    /// <summary>
    /// MCP-facing wrapper around <see cref="FindNearPointHandler"/>: transport/attribute plumbing only,
    /// with no logic of its own to keep untested by a running MCP server.
    /// </summary>
    [McpServerToolType]
    public sealed class FindNearPointTool
    {
        [McpServerTool(Name = "find_near_point")]
        [Description("Find the OpenStreetMap nodes nearest to a named place, optionally filtered by tags and a search radius. Use this when the user asks what's near, close to, or around a specific place.")]
        public static async Task<IReadOnlyList<NearPointMatch>> FindNearPoint(
            FindNearPointHandler handler,
            [Description("The place to search near, as a free-text name (e.g. 'Fredrikstad', 'Oslo City Hall'). Resolved to a geographic centroid by geocoding.")] string place,
            [Description("The search radius around the place, in meters.")] double radiusMeters,
            [Description("Optional OSM tag filters to match, as exact key-value pairs (e.g. amenity=cafe). Omit to match any node.")] Dictionary<string, string>? tags,
            [Description("The maximum number of nodes to return, ordered by distance from the place.")] int limit,
            CancellationToken cancellationToken)
            => await handler.FindAsync(place, radiusMeters, tags, limit, cancellationToken);
    }
}
