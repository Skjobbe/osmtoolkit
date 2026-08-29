using ModelContextProtocol.Server;
using System.ComponentModel;

namespace OsmToolkit.Mcp.Tools
{
    /// <summary>
    /// MCP-facing wrapper around <see cref="RouteBetweenPointsHandler"/>: transport/attribute plumbing only,
    /// with no logic of its own to keep untested by a running MCP server.
    /// </summary>
    [McpServerToolType]
    public sealed class RouteBetweenPointsTool
    {
        [McpServerTool(Name = "route_between_points")]
        [Description("Calculate the shortest route between two named places using OpenStreetMap way data. Use this when the user asks for directions, a route, or how to get from one place to another.")]
        public static async Task<RouteResult> RouteBetweenPoints(
            RouteBetweenPointsHandler handler,
            [Description("The starting place, as a free-text name (e.g. 'Fredrikstad', 'Oslo City Hall'). Resolved to a geographic coordinate by geocoding.")] string origin,
            [Description("The destination place, as a free-text name. Resolved to a geographic coordinate by geocoding.")] string destination,
            [Description("The mode of travel to route for: 'foot', 'bicycle', 'moped', or 'car'.")] string travelMode,
            [Description("Whether to avoid motorways/highways when routing.")] bool avoidMotorway,
            CancellationToken cancellationToken)
            => await McpErrorTranslator.TranslateKnownFailuresAsync(() => handler.RouteAsync(origin, destination, travelMode, avoidMotorway, cancellationToken));
    }
}
