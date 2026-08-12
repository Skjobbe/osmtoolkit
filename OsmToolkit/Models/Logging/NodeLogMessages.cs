using Microsoft.Extensions.Logging;

namespace OsmToolkit.Models.Logging
{
    internal static partial class NodeLogMessages
    {
        [LoggerMessage(Level = LogLevel.Trace, Message = "Node {NodeId} at ({Latitude}, {Longitude}) with tags: [{Tags}]")]
        internal static partial void LogNodeSummary(ILogger logger, long nodeId, double latitude, double longitude, string tags);

        [LoggerMessage(Level = LogLevel.Trace, Message = "Node {NodeId} at ({Latitude}, {Longitude})")]
        internal static partial void LogNodeSummaryTagless(ILogger logger, long nodeId, double latitude, double longitude);

        [LoggerMessage(Level = LogLevel.Trace, Message = "Node {NodeId} with tags: [{Tags}]")]
        internal static partial void LogNodeSummaryLocationless(ILogger logger, long nodeId, string tags);

        [LoggerMessage(Level = LogLevel.Debug, Message = "Node {NodeId} located at ({Latitude}, {Longitude}), created in v{Version}, CS {ChangeSet} by user {UserId} on {Timestamp}, visible={Visible} with tags:[{Tags}]")]
        internal static partial void LogNodeCreationInfo(ILogger logger, long nodeId, double latitude, double longitude, int version, long changeSet, long userId, DateTime timestamp, bool visible, string tags);


    }
}
