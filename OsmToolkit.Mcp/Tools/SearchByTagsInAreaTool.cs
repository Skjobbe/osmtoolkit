using ModelContextProtocol.Server;
using System.ComponentModel;

namespace OsmToolkit.Mcp.Tools
{
    /// <summary>
    /// MCP-facing wrapper around <see cref="SearchByTagsInAreaHandler"/>: transport/attribute plumbing only,
    /// with no logic of its own to keep untested by a running MCP server.
    /// </summary>
    [McpServerToolType]
    public sealed class SearchByTagsInAreaTool
    {
        [McpServerTool(Name = "search_by_tags_in_area")]
        [Description("Search for OpenStreetMap entities matching one or more tags within a named place (a city, address, or landmark). Use this when the user asks to find things of a certain kind — like cafes, bus stops, or hospitals — in or near a place. Returns matching entities with their tags and coordinates.")]
        public static async Task<IReadOnlyList<TagSearchMatch>> SearchByTagsInArea(
            SearchByTagsInAreaHandler handler,
            [Description("The place to search in, as a free-text name (e.g. 'Fredrikstad', 'Oslo City Hall'). Resolved to a geographic area by geocoding.")] string place,
            [Description("OSM tag filters to match, as key-value pairs. Use standard OSM keys/values (e.g. amenity=cafe, shop=supermarket). Omit a value to match any value for that key.")] Dictionary<string, string?> tags,
            CancellationToken cancellationToken)
            => await McpErrorTranslator.TranslateKnownFailuresAsync(() => handler.SearchAsync(place, tags, cancellationToken));
    }
}
