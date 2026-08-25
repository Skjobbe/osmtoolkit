using Microsoft.Extensions.Logging;

namespace OsmToolkit.Mcp.Tools.Logging
{
    internal static partial class RouteBetweenPointsLogMessages
    {
        [LoggerMessage(LogLevel.Debug, Message = "Routing from \"{Origin}\" to \"{Destination}\" by {TravelMode} (avoidMotorway={AvoidMotorway}).")]
        internal static partial void LogRouteStart(ILogger logger, string origin, string destination, string travelMode, bool avoidMotorway);

        [LoggerMessage(LogLevel.Debug, Message = "Route from \"{Origin}\" to \"{Destination}\": {TotalDistanceMeters}m, {WaypointCount} waypoint(s).")]
        internal static partial void LogRouteResult(ILogger logger, string origin, string destination, double totalDistanceMeters, int waypointCount);
    }
}
